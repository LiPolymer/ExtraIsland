using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ExtraIsland.Shared;

public static class RhesisHandler {
    public static bool HitokotoLimitation { get; set; }

    public class Instance {
        readonly Random _random = new Random();
        public RhesisData LegacyGet(RhesisDataSource rhesisDataSource = RhesisDataSource.All
            ,string? hitokotoRequestUrl = null, string? sainticRequestUrl = null, 
            string? onlineTxtUrl = null, int lengthLimitation = 0, int onlineTxtWeight = 20) {
            
            for (int i = 0; i <= 5; i++) {
                RhesisData dataFetched = rhesisDataSource switch {
                    RhesisDataSource.All => GetRandomSourceWithWeight(onlineTxtWeight),
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
                    RhesisDataSource.OnlineTxt => OnlineTxtData.Fetch(onlineTxtUrl).ToRhesisData(),
                    RhesisDataSource.SaintJinrishici => new RhesisData { Content = "处理时出现错误" },
                    RhesisDataSource.All => new RhesisData { Content = "处理时出现错误" },
                    _ => new RhesisData { Content = "处理时出现错误" }
                };
                
                if (lengthLimitation == 0 || dataFetched.Content.Length <= lengthLimitation) {
                    return dataFetched;
                }
            }
            
            return rhesisDataSource == RhesisDataSource.All ? HitokotoData.Fetch(hitokotoRequestUrl).ToRhesisData() 
                : new RhesisData { Content = "满足限制时遇到困难" };
        }

        private RhesisDataSource GetRandomSourceWithWeight(int onlineTxtWeight) {
            // 确保权重在0-100之间
            onlineTxtWeight = Math.Max(0, Math.Min(100, onlineTxtWeight));
            
            // 如果TXT权重为0或URL为空，则不考虑TXT源
            if (onlineTxtWeight == 0) {
                return GetTraditionalSource();
            }
            
            // 计算传统源的剩余权重
            int traditionalWeight = 100 - onlineTxtWeight;
            
            // 生成0-99之间的随机数
            int randomValue = _random.Next(100);
            
            // 如果随机数小于在线TXT权重，使用在线TXT
            if (randomValue < onlineTxtWeight) {
                return RhesisDataSource.OnlineTxt;
            }
            
            // 否则，使用传统源
            return GetTraditionalSource();
        }
        
        private RhesisDataSource GetTraditionalSource() {
            int randomValue = _random.Next(4);
            return randomValue switch {
                0 => RhesisDataSource.Jinrishici,  // 今日诗词接口概率较低
                1 => RhesisDataSource.Saint,
                2 => RhesisDataSource.Saint,
                _ => RhesisDataSource.Hitokoto
            };
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

public class OnlineTxtData {
    public string Sentence { get; set; } = string.Empty;

    public RhesisData ToRhesisData() {
        return new RhesisData {
            Author = string.Empty,
            Title = string.Empty,
            Content = Sentence,
            Source = "在线TXT文件",
            Catalog = "自定义",
        };
    }

    public static OnlineTxtData Fetch(string? requestUrl = null) {
        if (string.IsNullOrEmpty(requestUrl)) {
            return new OnlineTxtData {
                Sentence = "请在设置中配置TXT文件URL"
            };
        }

        try {
            var client = new HttpClient();
            var content = client.GetStringAsync(requestUrl).Result;
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length == 0) {
                return new OnlineTxtData {
                    Sentence = "TXT文件内容为空，请检查URL"
                };
            }

            var random = new Random();
            var randomLine = lines[random.Next(lines.Length)];
            
            return new OnlineTxtData {
                Sentence = randomLine.Trim()
            };
        }
        catch (Exception ex) {
            return new OnlineTxtData {
                Sentence = $"获取TXT文件时发生错误：请检查网络连接和URL有效性"
            };
        }
    }
}

public class SainticData {
    [JsonPropertyName("code")] 
    public int StatusCode { get; set; } = -1;

    [JsonPropertyName("data")] 
    public SainticRhesisData Data { get; set; } = new SainticRhesisData();

    [JsonPropertyName("msg")] 
    public string? Message { get; set; }
    
    [JsonPropertyName("q")] 
    public QueueInfoData QueueInfo { get; set; } = new QueueInfoData();

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
            requestUrl ??= "https://open.saintic.com/api/sentence/all.json";
            return new HttpClient()
                .GetFromJsonAsync<SainticData>(requestUrl)
                .Result!;
        }
        catch (Exception ex) {
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

    public static JinrishiciData Fetch(int lengthLimitation = 0) {
        try {
            return new HttpClient()
                .GetFromJsonAsync<JinrishiciData>("https://v1.jinrishici.com/all.json")
                .Result!;
        }
        catch (Exception ex) {
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
    Hitokoto = 3,
    [Description("在线TXT文件")]
    OnlineTxt = 4
}
