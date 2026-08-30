using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Media;

namespace ExtraIsland.Shared;

public sealed class HitokotoRhesisProvider : IRhesisProvider, IRhesisProviderSettingsFactory {
    public const string ProviderId = "hitokoto";
    public const string QueryOption = "query";

    static readonly HttpClient HttpClient = new HttpClient();
    readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1,1);
    DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;

    public string Id { get => ProviderId; }

    public string DisplayName { get => "一言"; }

    public string Description { get => "来自一言 API；请留意本来源中可能存在的不良内容。"; }

    public bool IsEnabledByDefault { get => false; }

    public int DefaultWeight { get => 1; }

    public async Task<RhesisData> FetchAsync(
        RhesisProviderConfig config,
        int lengthLimitation,
        CancellationToken cancellationToken = default) {
        await _requestLock.WaitAsync(cancellationToken);
        try {
            TimeSpan remaining = TimeSpan.FromMilliseconds(700) - (DateTimeOffset.UtcNow - _lastRequestAt);
            if (remaining > TimeSpan.Zero) {
                await Task.Delay(remaining,cancellationToken);
            }
            _lastRequestAt = DateTimeOffset.UtcNow;
        } finally {
            _requestLock.Release();
        }

        List<string> queryParts = [];
        if (lengthLimitation > 0) {
            queryParts.Add($"max_length={lengthLimitation}");
        }
        string customQuery = config.GetOption(QueryOption).Trim().TrimStart('?','&');
        if (!string.IsNullOrWhiteSpace(customQuery)) {
            queryParts.Add(customQuery);
        }

        string requestUrl = queryParts.Count == 0
            ? "https://v1.hitokoto.cn/"
            : $"https://v1.hitokoto.cn/?{string.Join("&",queryParts)}";
        HitokotoData data = await HttpClient.GetFromJsonAsync<HitokotoData>(requestUrl,cancellationToken)
            ?? throw new InvalidOperationException("一言 API 返回了空响应。");
        return data.ToRhesisData();
    }

    public Control CreateSettingsControl(RhesisProviderConfig config) {
        TextBox queryTextBox = new TextBox {
            Text = config.GetOption(QueryOption),
            PlaceholderText = "例如：c=i&c=k",
            MinWidth = 180
        };
        queryTextBox.TextChanged += (_,_) => config.SetOption(QueryOption,queryTextBox.Text);

        return new StackPanel {
            Spacing = 6,
            Margin = new Avalonia.Thickness(12,6),
            Children = {
                new TextBlock {
                    Text = "附加查询参数（不含“?”）。全局字数限制会自动转换为 max_length 参数。",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Gray
                },
                queryTextBox
            }
        };
    }
}

public sealed class JinrishiciRhesisProvider : IRhesisProvider {
    public const string ProviderId = "jinrishici";

    static readonly HttpClient HttpClient = new HttpClient();

    public string Id { get => ProviderId; }
    public string DisplayName { get => "今日诗词"; }
    public string Description { get => "来自今日诗词 API。"; }
    public bool IsEnabledByDefault { get => true; }
    public int DefaultWeight { get => 1; }

    public async Task<RhesisData> FetchAsync(
        RhesisProviderConfig config,
        int lengthLimitation,
        CancellationToken cancellationToken = default) {
        const string requestUrl = "https://v1.jinrishici.com/all.json";
        JinrishiciData data = await HttpClient.GetFromJsonAsync<JinrishiciData>(requestUrl,cancellationToken)
            ?? throw new InvalidOperationException("今日诗词 API 返回了空响应。");
        return data.ToRhesisData();
    }
}

public sealed class SainticRhesisProvider : IRhesisProvider, IRhesisProviderSettingsFactory {
    public const string ProviderId = "saintic";
    public const string PathOption = "path";

    static readonly HttpClient HttpClient = CreateHttpClient();

    public string Id { get => ProviderId; }
    public string DisplayName { get => "诏预"; }
    public string Description { get => "来自 Saintic 句子 API。"; }
    public bool IsEnabledByDefault { get => true; }
    public int DefaultWeight { get => 1; }

    public async Task<RhesisData> FetchAsync(
        RhesisProviderConfig config,
        int lengthLimitation,
        CancellationToken cancellationToken = default) {
        string path = config.GetOption(PathOption,"all").Trim().Trim('/').TrimEnd('.');
        if (path.EndsWith(".json",StringComparison.OrdinalIgnoreCase)) {
            path = path[..^5];
        }
        if (string.IsNullOrWhiteSpace(path)) {
            path = "all";
        }

        string requestUrl = $"https://hub.saintic.com/openservice/sentence/{path}.json";
        SainticData data = await HttpClient.GetFromJsonAsync<SainticData>(requestUrl,cancellationToken)
            ?? throw new InvalidOperationException("Saintic API 返回了空响应。");
        return data.ToRhesisData();
    }

    public Control CreateSettingsControl(RhesisProviderConfig config) {
        TextBox pathTextBox = new TextBox {
            Text = config.GetOption(PathOption),
            PlaceholderText = "all",
            MinWidth = 180
        };
        pathTextBox.TextChanged += (_,_) => config.SetOption(PathOption,pathTextBox.Text);

        return new StackPanel {
            Spacing = 6,
            Margin = new Avalonia.Thickness(12,6),
            Children = {
                new TextBlock {
                    Text = "接口路径：https://hub.saintic.com/openservice/sentence/{路径}.json",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Gray
                },
                pathTextBox
            }
        };
    }

    static HttpClient CreateHttpClient() {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("ExtraIsland/1.0"));
        return client;
    }
}

internal sealed class SainticData {
    [JsonPropertyName("code")]
    public int StatusCode { get; set; } = -1;

    [JsonPropertyName("data")]
    public SainticRhesisData Data { get; set; } = new SainticRhesisData();

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("remark")]
    public RemarkData Remark { get; set; } = new RemarkData();

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = Data.Author,
            Title = Data.Name,
            Content = Data.Sentence,
            Source = "诏预API",
            Catalog = $"{Data.Theme}-{Data.Catalog}"
        };
    }

    internal sealed class SainticRhesisData {
        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("author_pinyin")]
        public string AuthorPinyin { get; set; } = string.Empty;

        [JsonPropertyName("catalog")]
        public string Catalog { get; set; } = string.Empty;

        [JsonPropertyName("catalog_pinyin")]
        public string CatalogPinyin { get; set; } = string.Empty;

        [JsonPropertyName("ctime")]
        public int Ctime { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("sentence")]
        public string Sentence { get; set; } = string.Empty;

        [JsonPropertyName("src_url")]
        public string SrcUrl { get; set; } = string.Empty;

        [JsonPropertyName("theme")]
        public string Theme { get; set; } = string.Empty;

        [JsonPropertyName("theme_pinyin")]
        public string ThemePinyin { get; set; } = string.Empty;
    }

    internal sealed class RemarkData {
        [JsonPropertyName("q")]
        public QueueInfoData QueueInfo { get; set; } = new QueueInfoData();

        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        internal sealed class QueueInfoData {
            [JsonPropertyName("author")]
            public string Author { get; set; } = string.Empty;

            [JsonPropertyName("catalog")]
            public string Catalog { get; set; } = string.Empty;

            [JsonPropertyName("suffix")]
            public string Suffix { get; set; } = string.Empty;

            [JsonPropertyName("theme")]
            public string Theme { get; set; } = string.Empty;
        }
    }
}

internal sealed class JinrishiciData {
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = Author,
            Title = Origin,
            Content = Content,
            Source = "今日诗词API",
            Catalog = Category
        };
    }
}

internal sealed class HitokotoData {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("hitokoto")]
    public string Hitokoto { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("from_who")]
    public string FromWho { get; set; } = string.Empty;

    [JsonPropertyName("creator")]
    public string Creator { get; set; } = string.Empty;

    [JsonPropertyName("creator_uid")]
    public int CreatorUid { get; set; }

    [JsonPropertyName("reviewer")]
    public int Reviewer { get; set; }

    [JsonPropertyName("commit_from")]
    public string CommitFrom { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public int Length { get; set; }

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = FromWho,
            Title = From,
            Content = Hitokoto,
            Source = "一言API",
            Catalog = ConvertTypeToString(Type)
        };
    }

    static string ConvertTypeToString(string type) {
        return type switch {
            "a" => "动画",
            "b" => "漫画",
            "c" => "游戏",
            "d" => "文学",
            "e" => "原创",
            "f" => "网络",
            "g" => "其他",
            "h" => "影视",
            "i" => "诗词",
            "j" => "网易云",
            "k" => "哲学",
            "l" => "抖机灵",
            _ => string.Empty
        };
    }
}
