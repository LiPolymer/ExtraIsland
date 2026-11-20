using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace ExtraIsland.AuthorizeProvider;

public static class UsbCredentialHelper {
    public const string CredentialFileName = ".verify.extraisland.nfo";
    internal const int MaxCredentialFileBytes = 4096;

    public enum FileReadStatus {
        Success,
        NotFound,
        Invalid,
        Error
    }

    public readonly record struct FileReadResult(FileReadStatus Status, string? Content, string? ErrorMessage);

    public readonly record struct FileEnsureResult(bool Success, string? Content, string? FilePath, string? ErrorMessage);

    public readonly record struct PreparedCredential(bool Success, string PassHash, string Salt, bool IsFileModeEnabled, string? ErrorMessage);

    public static string GenerateSalt() {
        return Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
    }

    public static string GenerateFileToken() {
        return Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
    }

    public static string ComputeHash(string credential, string? salt) {
        ArgumentNullException.ThrowIfNull(credential);
        byte[] data = Encoding.UTF8.GetBytes(credential);

        if (string.IsNullOrEmpty(salt)) {
            return Convert.ToHexStringLower(SHA256.HashData(data));
        }

        return Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(salt), data));
    }

    public static bool VerifyHash(string? credential, string? expectedHash, string? salt) {
        if (string.IsNullOrEmpty(credential) || string.IsNullOrWhiteSpace(expectedHash)) {
            return false;
        }

        string actual = ComputeHash(credential, salt);
        string normalizedExpected = expectedHash.Trim().ToLowerInvariant();
        byte[] actualBytes = Encoding.UTF8.GetBytes(actual);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(normalizedExpected);
        return actualBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    public static string? TryGetCredentialFilePath(string? driveLetter) {
        string? normalized = UsbDriveScanner.NormalizeDriveLetter(driveLetter);
        return normalized is null ? null : Path.Combine($"{normalized}\\", CredentialFileName);
    }

    public static FileReadResult TryReadCredentialFile(string driveLetter) {
        string? path = TryGetCredentialFilePath(driveLetter);
        return path is null
            ? new FileReadResult(FileReadStatus.Error, null, "盘符格式无效")
            : TryReadCredentialFileAtPath(path);
    }

    internal static FileReadResult TryReadCredentialFileAtPath(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return new FileReadResult(FileReadStatus.Error, null, "路径无效");
        }

        try {
            if (!File.Exists(path)) {
                return new FileReadResult(FileReadStatus.NotFound, null, null);
            }

            FileInfo info = new FileInfo(path);
            if (info.Length <= 0 || info.Length > MaxCredentialFileBytes) {
                return new FileReadResult(FileReadStatus.Invalid, null, "密钥文件大小异常");
            }

            // Preserve raw content without trimming so legacy .verify.extraisland.nfo files hash identically.
            string content = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(content)) {
                return new FileReadResult(FileReadStatus.Invalid, null, "密钥文件内容为空");
            }

            return new FileReadResult(FileReadStatus.Success, content, null);
        } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException) {
            return new FileReadResult(FileReadStatus.Error, null, ex.Message);
        }
    }

    public static FileEnsureResult EnsureCredentialFile(UsbDriveInfo drive) {
        if (drive.DriveLetters.Count == 0) {
            return new FileEnsureResult(false, null, null, "该U盘没有可用盘符，无法写入密钥文件");
        }

        // Pass 1: Reuse an existing valid credential file on any partition.
        foreach (string letter in drive.DriveLetters) {
            string? path = TryGetCredentialFilePath(letter);
            if (path is null) {
                continue;
            }

            FileReadResult read = TryReadCredentialFileAtPath(path);
            if (read.Status == FileReadStatus.Success) {
                TrySetHiddenAttribute(path);
                return new FileEnsureResult(true, read.Content, path, null);
            }
        }

        // Pass 2: Create a fresh credential file on the first writable partition.
        string token = GenerateFileToken();
        string? lastError = null;

        foreach (string letter in drive.DriveLetters) {
            string? path = TryGetCredentialFilePath(letter);
            if (path is null) {
                continue;
            }

            try {
                if (File.Exists(path)) {
                    // Hidden or ReadOnly corrupt/empty files block overwriting via File.WriteAllText on Windows.
                    FileAttributes attrs = File.GetAttributes(path);
                    if ((attrs & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0) {
                        File.SetAttributes(path, attrs & ~(FileAttributes.ReadOnly | FileAttributes.Hidden));
                    }
                }

                File.WriteAllText(path, token, new UTF8Encoding(false));
                TrySetHiddenAttribute(path);

                FileReadResult verify = TryReadCredentialFileAtPath(path);
                if (verify.Status == FileReadStatus.Success) {
                    return new FileEnsureResult(true, verify.Content, path, null);
                }

                lastError = verify.ErrorMessage;
            } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException) {
                lastError = ex.Message;
            }
        }

        string message = string.IsNullOrWhiteSpace(lastError)
            ? "无法在U盘上写入密钥文件（设备可能为只读、无权限或已拔出）。"
            : $"无法在U盘上写入密钥文件（设备可能为只读、无权限或已拔出）：{lastError}";
        return new FileEnsureResult(false, null, null, message);
    }

    public static bool VerifyDrive(UsbDriveInfo drive, UsbDriveAuthorizerSettings settings) {
        if (string.IsNullOrWhiteSpace(settings.PassHash)) {
            return false;
        }

        if (settings.IsFileModeEnabled) {
            foreach (string letter in drive.DriveLetters) {
                FileReadResult read = TryReadCredentialFile(letter);
                if (read.Status == FileReadStatus.Success &&
                    VerifyHash(read.Content, settings.PassHash, settings.Salt)) {
                    return true;
                }
            }

            return false;
        }

        return !string.IsNullOrWhiteSpace(drive.SerialNumber) &&
               VerifyHash(drive.SerialNumber, settings.PassHash, settings.Salt);
    }

    public static PreparedCredential PrepareBind(UsbDriveInfo drive, bool useFileMode) {
        if (useFileMode) {
            FileEnsureResult ensureResult = EnsureCredentialFile(drive);
            if (!ensureResult.Success || string.IsNullOrEmpty(ensureResult.Content)) {
                return new PreparedCredential(
                    false,
                    string.Empty,
                    string.Empty,
                    true,
                    ensureResult.ErrorMessage ?? "写入或读取密钥文件失败。");
            }

            string salt = GenerateSalt();
            string hash = ComputeHash(ensureResult.Content, salt);
            return new PreparedCredential(true, hash, salt, true, null);
        }

        if (string.IsNullOrWhiteSpace(drive.SerialNumber)) {
            return new PreparedCredential(
                false,
                string.Empty,
                string.Empty,
                false,
                "该U盘缺少有效的唯一硬件序列号，请更换正规U盘或改用“文件模式”。");
        }

        string snSalt = GenerateSalt();
        string snHash = ComputeHash(drive.SerialNumber, snSalt);
        return new PreparedCredential(true, snHash, snSalt, false, null);
    }

    public static (bool Success, string? ErrorMessage) BindDrive(
        UsbDriveInfo drive,
        bool useFileMode,
        UsbDriveAuthorizerSettings settings) {
        PreparedCredential prepared = PrepareBind(drive, useFileMode);
        if (!prepared.Success) {
            return (false, prepared.ErrorMessage);
        }

        settings.Salt = prepared.Salt;
        settings.PassHash = prepared.PassHash;
        settings.IsFileModeEnabled = prepared.IsFileModeEnabled;
        return (true, null);
    }

    static void TrySetHiddenAttribute(string path) {
        try {
            FileAttributes attrs = File.GetAttributes(path);
            if ((attrs & FileAttributes.Hidden) == 0) {
                File.SetAttributes(path, attrs | FileAttributes.Hidden);
            }
        } catch {
            // Filesystem attribute restrictions should not break credential binding.
        }
    }
}