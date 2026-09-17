[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertFrom-VersionCharacter {
    param(
        [Parameter(Mandatory)]
        [string] $Value
    )

    if ($Value -cmatch '^[0-9]$') {
        return [int]::Parse($Value, [Globalization.CultureInfo]::InvariantCulture)
    }

    if ($Value -cmatch '^[a-z]$') {
        return [int][char]$Value - [int][char]'a' + 10
    }

    throw "Invalid version character: $Value"
}

function Assert-RequiredEnvironmentVariable {
    param(
        [Parameter(Mandatory)]
        [string] $Name
    )

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Required environment variable $Name is missing."
    }
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $repositoryRoot

try {
    $cipxDirectory = Join-Path $repositoryRoot 'ExtraIsland/cipx'
    $sourcePackagePath = Join-Path $cipxDirectory 'ExtraIsland.cipx'
    $checksumPath = Join-Path $cipxDirectory 'checksums.md'
    $assetName = 'ink.lipoly.ext.extraisland.cipx'
    $assetPath = Join-Path $cipxDirectory $assetName

    # Prevent a failed run from exposing an output left by a reused runner workspace.
    foreach ($staleOutput in @($sourcePackagePath, $checksumPath, $assetPath)) {
        if (Test-Path -LiteralPath $staleOutput) {
            Remove-Item -LiteralPath $staleOutput -Force
        }
    }

    Assert-RequiredEnvironmentVariable 'CI_COMMIT_TITLE'

    $titlePattern = '^release: (?<releaseName>(?<channel>[ABR])(?<major>[0-9]+)\.(?<minor>[0-9a-z])(?<patch>[0-9a-z])(?:fix(?<fix>[0-9a-z]))?)$'
    $titleMatch = [regex]::Match(
        $env:CI_COMMIT_TITLE,
        $titlePattern,
        [Text.RegularExpressions.RegexOptions]::CultureInvariant
    )

    if (-not $titleMatch.Success) {
        throw "Commit title does not match the release format: $($env:CI_COMMIT_TITLE)"
    }

    $releaseName = $titleMatch.Groups['releaseName'].Value
    $releaseChannel = $titleMatch.Groups['channel'].Value
    $major = [int64]::Parse(
        $titleMatch.Groups['major'].Value,
        [Globalization.CultureInfo]::InvariantCulture
    )
    $minor = ConvertFrom-VersionCharacter $titleMatch.Groups['minor'].Value
    $patch = ConvertFrom-VersionCharacter $titleMatch.Groups['patch'].Value
    $fix = if ($titleMatch.Groups['fix'].Success) {
        ConvertFrom-VersionCharacter $titleMatch.Groups['fix'].Value
    }
    else {
        0
    }
    $expectedVersionParts = @($major, $minor, $patch, $fix)

    $manifestPath = Join-Path $repositoryRoot 'ExtraIsland/manifest.yml'
    $manifestText = Get-Content -LiteralPath $manifestPath -Raw
    $manifestMatch = [regex]::Match(
        $manifestText,
        '(?m)^version:\s*["'']?(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)["'']?\s*$'
    )

    if (-not $manifestMatch.Success) {
        throw 'ExtraIsland/manifest.yml does not contain a four-part numeric version.'
    }

    $manifestVersion = $manifestMatch.Groups['version'].Value
    $manifestVersionParts = @(
        $manifestVersion.Split('.') | ForEach-Object {
            [int64]::Parse($_, [Globalization.CultureInfo]::InvariantCulture)
        }
    )

    if ((Compare-Object $expectedVersionParts $manifestVersionParts -SyncWindow 0)) {
        $expectedVersion = $expectedVersionParts -join '.'
        throw "Release version $releaseName maps to $expectedVersion, but manifest.yml declares $manifestVersion."
    }

    Write-Host "Validated release version: $releaseName -> $manifestVersion"

    $previousVersion = $null
    $releaseNotes = $null

    if ($releaseChannel -in @('B', 'R')) {
        foreach ($name in @('CI_COMMIT_SHA', 'CI_PROJECT_ID', 'CI_PROJECT_URL', 'CI_API_V4_URL', 'CI_JOB_TOKEN')) {
            Assert-RequiredEnvironmentVariable $name
        }

        & git show-ref --verify --quiet "refs/tags/$manifestVersion"
        $showRefExitCode = $LASTEXITCODE
        if ($showRefExitCode -eq 0) {
            throw "Tag $manifestVersion already exists."
        }
        if ($showRefExitCode -ne 1) {
            throw "Unable to check whether tag $manifestVersion exists (git exit code $showRefExitCode)."
        }

        $parentRef = "$($env:CI_COMMIT_SHA)^"
        $versionTags = @(& git tag --merged $parentRef --sort=-version:refname)
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to find tags reachable from $parentRef."
        }

        $previousVersion = $versionTags |
            Where-Object { $_ -cmatch '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' } |
            Select-Object -First 1

        if ([string]::IsNullOrWhiteSpace($previousVersion)) {
            throw "No previous four-part version tag is reachable from $parentRef."
        }

        $compareUrl = '{0}/-/compare/{1}...{2}?from_project_id={3}' -f @(
            $env:CI_PROJECT_URL,
            $previousVersion,
            $manifestVersion,
            $env:CI_PROJECT_ID
        )
        $changeLog = [string]$env:CI_COMMIT_DESCRIPTION
        $changeLog = $changeLog.TrimEnd("`r", "`n")

        $releaseNotes = @(
            $changeLog
            ''
            ('**完整变化**: [`{0}...{1}`]({2})' -f $previousVersion, $manifestVersion, $compareUrl)
            ''
            '> [!IMPORTANT]'
            '> 下载后请注意核对MD5'
            ''
            '| 文件名                          | MD5     |'
            '|---------------------------------|---------|'
            ('| {0} | `{1}` |' -f $assetName, '{md5}')
            ''
            ('<!-- CLASSISLAND_PKG_MD5 {{"{0}":"{1}"}} -->' -f $assetName, '{md5}')
        ) -join "`n"
    }

    & dotnet publish '-p:CreateCipx=true'
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }

    foreach ($generatedPath in @($sourcePackagePath, $checksumPath)) {
        if (-not (Test-Path -LiteralPath $generatedPath -PathType Leaf)) {
            throw "Expected publish output is missing: $generatedPath"
        }
    }

    $checksumText = Get-Content -LiteralPath $checksumPath -Raw
    $checksumMatch = [regex]::Match(
        $checksumText,
        '"ExtraIsland\.cipx"\s*:\s*"(?<md5>[0-9A-Fa-f]{32})"'
    )
    if (-not $checksumMatch.Success) {
        throw 'Unable to read the ExtraIsland.cipx MD5 from checksums.md.'
    }

    $expectedMd5 = $checksumMatch.Groups['md5'].Value.ToUpperInvariant()
    Move-Item -LiteralPath $sourcePackagePath -Destination $assetPath

    $actualMd5 = (Get-FileHash -LiteralPath $assetPath -Algorithm MD5).Hash.ToUpperInvariant()
    if ($actualMd5 -cne $expectedMd5) {
        throw "Package MD5 mismatch: checksums.md contains $expectedMd5, calculated $actualMd5."
    }

    Write-Host "Prepared pipeline artifact: $assetName ($actualMd5)"

    if ($releaseChannel -eq 'A') {
        Write-Host 'A channel build: skipping Git tag and GitLab Release creation.'
        return
    }

    $releaseNotes = $releaseNotes.Replace('{md5}', $actualMd5)
    $packageUrl = '{0}/projects/{1}/packages/generic/{2}/{3}/{4}' -f @(
        $env:CI_API_V4_URL,
        $env:CI_PROJECT_ID,
        'ink.lipoly.ext.extraisland',
        $manifestVersion,
        $assetName
    )
    $headers = @{ 'JOB-TOKEN' = $env:CI_JOB_TOKEN }

    Write-Host "Uploading release package for version $manifestVersion."
    Invoke-RestMethod `
        -Uri $packageUrl `
        -Method Put `
        -Headers $headers `
        -InFile $assetPath `
        -ContentType 'application/octet-stream' | Out-Null

    $releasePayload = @{
        name = $releaseName
        tag_name = $manifestVersion
        ref = $env:CI_COMMIT_SHA
        description = $releaseNotes
        assets = @{
            links = @(
                @{
                    name = $assetName
                    url = $packageUrl
                    direct_asset_path = "/$assetName"
                    link_type = 'package'
                }
            )
        }
    } | ConvertTo-Json -Depth 6

    Write-Host "Creating GitLab Release $releaseName with tag $manifestVersion."
    Invoke-RestMethod `
        -Uri "$($env:CI_API_V4_URL)/projects/$($env:CI_PROJECT_ID)/releases" `
        -Method Post `
        -Headers $headers `
        -Body ([Text.Encoding]::UTF8.GetBytes($releasePayload)) `
        -ContentType 'application/json; charset=utf-8' | Out-Null
}
finally {
    Pop-Location
}
