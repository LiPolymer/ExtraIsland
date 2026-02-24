using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

public static class RhesisHandler {
    public static bool HitokotoLimitation { get; set; }

    public class Instance {
        readonly Random _random = new Random();
        public RhesisData LegacyGet(RhesisDataSource rhesisDataSource = RhesisDataSource.All
            ,string? hitokotoRequestUrl = null,string? sainticRequestUrl = null, int lengthLimitation = 0) {
            for (int i = 0; i <= 5; i++) {
                RhesisData dataFetched =  rhesisDataSource switch {
                    //TODO: 使用基于权重的随机方法(在EiUtil内实现)
                    RhesisDataSource.All => _random.Next(4) switch {
                        0 => RhesisDataSource.Jinrishici,
                        //今日诗词接口内容较少,故概率较低
                        1 => RhesisDataSource.Saint,
                        2 => RhesisDataSource.Saint,
                        3 => RhesisDataSource.Hitokoto,
                        4 => RhesisDataSource.Hitokoto,
                        _ => rhesisDataSource
                    },
                    RhesisDataSource.SaintJinrishici => _random.Next(2) switch {
                        0 => RhesisDataSource.Jinrishici,
                        //今日诗词接口内容较少,故概率较低
                        1 => RhesisDataSource.Saint,
                        2 => RhesisDataSource.Saint,
                        _ => rhesisDataSource
                    },
                    _ => rhesisDataSource
                } switch {
                    RhesisDataSource.Jinrishici => JinrishiciData.Fetch().ToRhesisData(),
                    RhesisDataSource.Saint => SainticData.Fetch(sainticRequestUrl).ToRhesisData(),
                    RhesisDataSource.Hitokoto => HitokotoData.Fetch(hitokotoRequestUrl).ToRhesisData(),
                    RhesisDataSource.SaintJinrishici => new RhesisData { Content = "处理时出现错误" },
                    RhesisDataSource.All => new RhesisData { Content = "处理时出现错误" },
                    _ => new RhesisData { Content = "处理时出现错误" }
                };
                if (lengthLimitation == 0 | dataFetched.Content.Length <= lengthLimitation) {
                    return dataFetched;
                }
            }
            return rhesisDataSource == RhesisDataSource.All ? HitokotoData.Fetch(hitokotoRequestUrl).ToRhesisData() 
                : new RhesisData { Content = "满足限制时遇到困难" };
        }
    }
}

public class RhesisData {
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public string Catalog { get; set; } = string.Empty;
}

public class SainticData {

    [JsonPropertyName("code")] 
    public int StatusCode { get; set; } = -1;

    [JsonPropertyName("data")] 
    public SainticRhesisData Data { get; set; } = new SainticRhesisData();

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("remark")]
    public RemarkData Remark { get; set; } = new RemarkData();

    public class SainticRhesisData {

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("author_pinyin")]
        public string AuthorPinyin { get; set; } = string.Empty;

        [JsonPropertyName("catalog")] 
        public string Catalog { get; set; } = string.Empty;

        [JsonPropertyName("catalog_pinyin")] 
        public string CatalogPinyin { get; set; } = string.Empty;

        [JsonPropertyName("ctime")] 
        public int Ctime { get; set; } = 0;

        [JsonPropertyName("id")] 
        public int Id { get; set; } = 0;

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

    public class RemarkData {
        [JsonPropertyName("q")]
        public QueueInfoData QueueInfo { get; set; } = new QueueInfoData();

        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        public class QueueInfoData {

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

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = Data.Author,
            Title = Data.Name,
            Content = Data.Sentence,
            Source = "诏预API",
            Catalog = $"{Data.Theme}-{Data.Catalog}",
        };
    }

    public static SainticData Fetch(string? requestUrl = null) {
        try {
            requestUrl ??= "https://hub.saintic.com/openservice/sentence/all.json";
            return new HttpClient {
                    DefaultRequestHeaders = {
                        UserAgent = {
                            ProductInfoHeaderValue.Parse("ExtraIsland/1.0")
                        }
                    }
                }
                .GetFromJsonAsync<SainticData>(requestUrl)
                .Result!;
        }
        catch (Exception ex) {
            GlobalConstants.HostInterfaces.PluginLogger!.LogWarning(ex,$"获取时发生错误，URL: {requestUrl}");
            return new SainticData {
                Data = new SainticRhesisData {
                    Sentence = $"获取时发生错误"
                }
            };
        }
    }
}

public class JinrishiciData {

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
            Catalog = Category,
        };
    }

    public static JinrishiciData Fetch(int lengthLimitation = 0)
    {
        var requestUrl = "https://v1.jinrishici.com/all.json";
        try {
            return new HttpClient()
                .GetFromJsonAsync<JinrishiciData>(requestUrl)
                .Result!;
        }
        catch (Exception ex) {
            GlobalConstants.HostInterfaces.PluginLogger!.LogWarning(ex,$"获取时发生错误，URL: {requestUrl}");
            return new JinrishiciData {
                Content = "获取时发生错误"
            };
        }
    }
}

public class HitokotoData {
    [JsonPropertyName("id")] 
    public int Id { get; set; } = 0;

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
    public int CreatorUid { get; set; } = 0;

    [JsonPropertyName("reviewer")] 
    public int Reviewer { get; set; } = 0;

    [JsonPropertyName("commit_from")] 
    public string CommitFrom { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("length")] 
    public int Length { get; set; } = 0;

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = FromWho,
            Title = From,
            Content = Hitokoto,
            Source = "一言API",
            Catalog = ConvertTypeToString(Type),
        };
    }

    static string ConvertTypeToString(string type) {
        string result = type switch {
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
        return result;
    }

    public static HitokotoData Fetch(string? requestUrl = null) {
        requestUrl ??= "https://v1.hitokoto.cn/";
        if (RhesisHandler.HitokotoLimitation) {
            return new HitokotoData {
                Hitokoto = "已达到一言接口限制"
            };
        }
        RhesisHandler.HitokotoLimitation = true;
        new Thread(() => {
            Thread.Sleep(700);
            RhesisHandler.HitokotoLimitation = false;
        }).Start();
        try {
            return new HttpClient()
                .GetFromJsonAsync<HitokotoData>(requestUrl)
                .Result!;
        }
        catch (Exception ex) {
            GlobalConstants.HostInterfaces.PluginLogger!.LogWarning(ex,$"获取时发生错误，URL: {requestUrl}");
            return new HitokotoData {
                Hitokoto = $"获取时发生错误"
            };
        }
    }
}

public enum RhesisDataSource {
    [Description("仅古诗文(诏预 + 今日诗词)")]
    SaintJinrishici = -1,
    [Description("全部启用")]
    All = 0,
    [Description("诏预")]
    Saint = 1,
    [Description("今日诗词")]
    Jinrishici = 2,
    [Description("一言")]
    Hitokoto = 3
}