using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;

namespace ExtraIsland.Shared;

public static class RhesisHandler
{
    public static bool HitokotoLimitation { get; set; }
    public static PrefetchedQuoteManager PrefetchManager { get; } = new PrefetchedQuoteManager();

    public class PrefetchedQuoteManager
    {
        private readonly ConcurrentQueue<RhesisData> _hitokotoQuotes = new ConcurrentQueue<RhesisData>();
        private readonly ConcurrentQueue<RhesisData> _jinrishiciQuotes = new ConcurrentQueue<RhesisData>();
        private readonly ConcurrentQueue<RhesisData> _sainticQuotes = new ConcurrentQueue<RhesisData>();
        private readonly ConcurrentQueue<RhesisData> _onlineTxtQuotes = new ConcurrentQueue<RhesisData>();

        private DateTime _lastHitokotoPrefetch = DateTime.MinValue;
        private DateTime _lastJinrishiciPrefetch = DateTime.MinValue;
        private DateTime _lastSainticPrefetch = DateTime.MinValue;

        private readonly Random _random = new Random();
        private readonly object _lockObject = new object();

        public void PrefetchQuotes(bool enableHitokoto, bool enableJinrishici, bool enableSaint, bool enableOnlineTxt,
            int maxPrefetchedQuotes, int prefetchIntervalSeconds,
            string? hitokotoRequestUrl, string? sainticRequestUrl, string? onlineTxtUrl)
        {

            // If update interval is less than prefetch interval, don't prefetch
            if (prefetchIntervalSeconds <= 0)
            {
                return;
            }

            var now = DateTime.Now;

            // Prefetch Hitokoto
            if (enableHitokoto && _hitokotoQuotes.Count < maxPrefetchedQuotes &&
                (now - _lastHitokotoPrefetch).TotalSeconds >= prefetchIntervalSeconds)
            {
                lock (_lockObject)
                {
                    if ((now - _lastHitokotoPrefetch).TotalSeconds >= prefetchIntervalSeconds)
                    {
                        _lastHitokotoPrefetch = now;
                        try
                        {
                            var data = HitokotoData.Fetch(hitokotoRequestUrl).ToRhesisData();
                            if (!string.IsNullOrEmpty(data.Content) && data.Content != "获取时发生错误" && data.Content != "已达到一言接口限制")
                            {
                                _hitokotoQuotes.Enqueue(data);
                            }
                        }
                        catch
                        {
                            // Ignore prefetch errors
                        }
                    }
                }
            }

            // Prefetch Jinrishici
            if (enableJinrishici && _jinrishiciQuotes.Count < maxPrefetchedQuotes &&
                (now - _lastJinrishiciPrefetch).TotalSeconds >= prefetchIntervalSeconds)
            {
                lock (_lockObject)
                {
                    if ((now - _lastJinrishiciPrefetch).TotalSeconds >= prefetchIntervalSeconds)
                    {
                        _lastJinrishiciPrefetch = now;
                        try
                        {
                            var data = JinrishiciData.Fetch().ToRhesisData();
                            if (!string.IsNullOrEmpty(data.Content) && data.Content != "获取时发生错误")
                            {
                                _jinrishiciQuotes.Enqueue(data);
                            }
                        }
                        catch
                        {
                            // Ignore prefetch errors
                        }
                    }
                }
            }

            // Prefetch Saintic
            if (enableSaint && _sainticQuotes.Count < maxPrefetchedQuotes &&
                (now - _lastSainticPrefetch).TotalSeconds >= prefetchIntervalSeconds)
            {
                lock (_lockObject)
                {
                    if ((now - _lastSainticPrefetch).TotalSeconds >= prefetchIntervalSeconds)
                    {
                        _lastSainticPrefetch = now;
                        try
                        {
                            var data = SainticData.Fetch(sainticRequestUrl).ToRhesisData();
                            if (!string.IsNullOrEmpty(data.Content) && data.Content != "获取时发生错误")
                            {
                                _sainticQuotes.Enqueue(data);
                            }
                        }
                        catch
                        {
                            // Ignore prefetch errors
                        }
                    }
                }
            }

            // Prefetch OnlineTxt (no rate limiting required)
            if (enableOnlineTxt && _onlineTxtQuotes.Count < maxPrefetchedQuotes && !string.IsNullOrEmpty(onlineTxtUrl))
            {
                try
                {
                    var data = OnlineTxtData.Fetch(onlineTxtUrl).ToRhesisData();
                    if (!string.IsNullOrEmpty(data.Content) && !data.Content.Contains("发生错误") && !data.Content.Contains("请在设置中配置"))
                    {
                        _onlineTxtQuotes.Enqueue(data);
                    }
                }
                catch
                {
                    // Ignore prefetch errors
                }
            }

            // Trim queues if they're too large
            while (_hitokotoQuotes.Count > maxPrefetchedQuotes && _hitokotoQuotes.TryDequeue(out _)) { }
            while (_jinrishiciQuotes.Count > maxPrefetchedQuotes && _jinrishiciQuotes.TryDequeue(out _)) { }
            while (_sainticQuotes.Count > maxPrefetchedQuotes && _sainticQuotes.TryDequeue(out _)) { }
            while (_onlineTxtQuotes.Count > maxPrefetchedQuotes && _onlineTxtQuotes.TryDequeue(out _)) { }
        }

        public RhesisData? GetQuote(RhesisDataSource source)
        {
            switch (source)
            {
                case RhesisDataSource.Hitokoto:
                    if (_hitokotoQuotes.TryDequeue(out var hitokotoQuote))
                    {
                        return hitokotoQuote;
                    }
                    break;
                case RhesisDataSource.Jinrishici:
                    if (_jinrishiciQuotes.TryDequeue(out var jinrishiciQuote))
                    {
                        return jinrishiciQuote;
                    }
                    break;
                case RhesisDataSource.Saint:
                    if (_sainticQuotes.TryDequeue(out var sainticQuote))
                    {
                        return sainticQuote;
                    }
                    break;
                case RhesisDataSource.OnlineTxt:
                    if (_onlineTxtQuotes.TryDequeue(out var onlineTxtQuote))
                    {
                        return onlineTxtQuote;
                    }
                    break;
            }
            return null;
        }

        public RhesisData? GetAnyQuote()
        {
            // Try to get any available quote from any source
            var availableQueues = new List<ConcurrentQueue<RhesisData>>();

            if (_hitokotoQuotes.Count > 0) availableQueues.Add(_hitokotoQuotes);
            if (_jinrishiciQuotes.Count > 0) availableQueues.Add(_jinrishiciQuotes);
            if (_sainticQuotes.Count > 0) availableQueues.Add(_sainticQuotes);
            if (_onlineTxtQuotes.Count > 0) availableQueues.Add(_onlineTxtQuotes);

            if (availableQueues.Count == 0) return null;

            var selectedQueue = availableQueues[_random.Next(availableQueues.Count)];
            if (selectedQueue.TryDequeue(out var quote))
            {
                return quote;
            }

            return null;
        }

        public RhesisData? GetQuoteForCombinedSource(RhesisDataSource combinedSource,
            bool enableHitokoto, bool enableJinrishici, bool enableSaint, bool enableOnlineTxt)
        {
            // Create a list of available sources with quotes based on enabled settings
            var availableSources = new List<RhesisDataSource>();

            if (_hitokotoQuotes.Count > 0 && enableHitokoto && combinedSource == RhesisDataSource.All)
            {
                availableSources.Add(RhesisDataSource.Hitokoto);
            }

            if (_jinrishiciQuotes.Count > 0 && enableJinrishici &&
               (combinedSource == RhesisDataSource.All || combinedSource == RhesisDataSource.SaintJinrishici))
            {
                availableSources.Add(RhesisDataSource.Jinrishici);
            }

            if (_sainticQuotes.Count > 0 && enableSaint &&
               (combinedSource == RhesisDataSource.All || combinedSource == RhesisDataSource.SaintJinrishici))
            {
                availableSources.Add(RhesisDataSource.Saint);
            }

            if (_onlineTxtQuotes.Count > 0 && enableOnlineTxt && combinedSource == RhesisDataSource.All)
            {
                availableSources.Add(RhesisDataSource.OnlineTxt);
            }

            if (availableSources.Count == 0)
            {
                return GetAnyQuote(); // Fall back to any available quote
            }

            // Select a random source and get a quote from it
            var selectedSource = availableSources[_random.Next(availableSources.Count)];
            return GetQuote(selectedSource);
        }

        public int GetTotalQuoteCount()
        {
            return _hitokotoQuotes.Count + _jinrishiciQuotes.Count + _sainticQuotes.Count + _onlineTxtQuotes.Count;
        }

        public Dictionary<RhesisDataSource, int> GetQuoteCounts()
        {
            return new Dictionary<RhesisDataSource, int> {
                { RhesisDataSource.Hitokoto, _hitokotoQuotes.Count },
                { RhesisDataSource.Jinrishici, _jinrishiciQuotes.Count },
                { RhesisDataSource.Saint, _sainticQuotes.Count },
                { RhesisDataSource.OnlineTxt, _onlineTxtQuotes.Count }
            };
        }
    }

    public class Instance
    {
        readonly Random _random = new Random();

        public RhesisData LegacyGet(
            RhesisDataSource rhesisDataSource = RhesisDataSource.All,
            string? hitokotoRequestUrl = null,
            string? sainticRequestUrl = null,
            string? onlineTxtUrl = null,
            int lengthLimitation = 0,
            int onlineTxtWeight = 20,
            Dictionary<RhesisDataSource, int>? customWeights = null,
            bool enableHitokoto = true,
            bool enableJinrishici = true,
            bool enableSaint = true,
            bool enableOnlineTxt = true)
        {

            // First, try to get data from the online API
            RhesisData? result = null;
            bool networkError = false;

            try
            {
                RhesisDataSource selectedSource;

                if (rhesisDataSource == RhesisDataSource.All && customWeights != null && customWeights.Count > 0)
                {
                    selectedSource = GetSourceBasedOnCustomWeights(customWeights);
                }
                else if (rhesisDataSource == RhesisDataSource.All)
                {
                    selectedSource = GetRandomSourceWithWeight(onlineTxtWeight);
                }
                else if (rhesisDataSource == RhesisDataSource.SaintJinrishici)
                {
                    selectedSource = _random.Next(5) < 2 ? RhesisDataSource.Jinrishici : RhesisDataSource.Saint;
                }
                else
                {
                    selectedSource = rhesisDataSource;
                }

                // Skip sources that are disabled
                if ((selectedSource == RhesisDataSource.Hitokoto && !enableHitokoto) ||
                    (selectedSource == RhesisDataSource.Jinrishici && !enableJinrishici) ||
                    (selectedSource == RhesisDataSource.Saint && !enableSaint) ||
                    (selectedSource == RhesisDataSource.OnlineTxt && !enableOnlineTxt))
                {
                    throw new Exception("Selected source is disabled");
                }

                result = FetchFromSource(selectedSource, hitokotoRequestUrl, sainticRequestUrl, onlineTxtUrl);

                // Check if the result indicates a network error
                if (result.Content.Contains("发生错误") ||
                    result.Content.Contains("请检查网络") ||
                    result.Content.Contains("请在设置中配置"))
                {
                    networkError = true;
                }
            }
            catch (Exception)
            {
                networkError = true;
            }

            // If we got a valid result and it meets the length limitation, return it
            if (!networkError && result != null &&
                (lengthLimitation == 0 || result.Content.Length <= lengthLimitation))
            {
                return result;
            }

            // If we encountered a network error or length limitation issue, use a prefetched quote
            RhesisData? prefetchedQuote = null;

            if (rhesisDataSource == RhesisDataSource.All || rhesisDataSource == RhesisDataSource.SaintJinrishici)
            {
                prefetchedQuote = PrefetchManager.GetQuoteForCombinedSource(
                    rhesisDataSource, enableHitokoto, enableJinrishici, enableSaint, enableOnlineTxt);
            }
            else
            {
                prefetchedQuote = PrefetchManager.GetQuote(rhesisDataSource);
            }

            // If we have a prefetched quote that meets the length limitation, use it
            if (prefetchedQuote != null &&
                (lengthLimitation == 0 || prefetchedQuote.Content.Length <= lengthLimitation))
            {
                return prefetchedQuote;
            }

            // If we still don't have a valid quote, try any available prefetched quote regardless of source
            prefetchedQuote = PrefetchManager.GetAnyQuote();
            if (prefetchedQuote != null)
            {
                return prefetchedQuote;
            }

            // If all else fails, return a fallback message
            return new RhesisData
            {
                Content = networkError ?
                    "网络连接失败，预请求缓存为空，请检查网络连接" :
                    "满足限制时遇到困难，请调整字数限制"
            };
        }

        private RhesisData FetchFromSource(
            RhesisDataSource source,
            string? hitokotoRequestUrl,
            string? sainticRequestUrl,
            string? onlineTxtUrl)
        {

            switch (source)
            {
                case RhesisDataSource.Jinrishici:
                    return JinrishiciData.Fetch().ToRhesisData();
                case RhesisDataSource.Saint:
                    return SainticData.Fetch(sainticRequestUrl).ToRhesisData();
                case RhesisDataSource.Hitokoto:
                    return HitokotoData.Fetch(hitokotoRequestUrl).ToRhesisData();
                case RhesisDataSource.OnlineTxt:
                    return OnlineTxtData.Fetch(onlineTxtUrl).ToRhesisData();
                default:
                    return new RhesisData { Content = "未知数据源" };
            }
        }

        private RhesisDataSource GetSourceBasedOnCustomWeights(Dictionary<RhesisDataSource, int> weights)
        {
            // Calculate total weight of enabled sources
            int totalWeight = 0;
            foreach (var weight in weights.Values)
            {
                totalWeight += weight;
            }

            if (totalWeight <= 0)
            {
                // No sources enabled or all weights are 0, default to Hitokoto
                return RhesisDataSource.Hitokoto;
            }

            // Generate a random value based on total weight
            int randomValue = _random.Next(totalWeight);

            // Find the selected source based on the random value
            int currentSum = 0;
            foreach (var pair in weights)
            {
                currentSum += pair.Value;
                if (randomValue < currentSum)
                {
                    return pair.Key;
                }
            }

            // Fallback (should not happen)
            return RhesisDataSource.Hitokoto;
        }

        private RhesisDataSource GetRandomSourceWithWeight(int onlineTxtWeight)
        {
            // Ensure weight is in 0-100 range
            onlineTxtWeight = Math.Max(0, Math.Min(100, onlineTxtWeight));

            // If TXT weight is 0, use traditional sources
            if (onlineTxtWeight == 0)
            {
                return GetTraditionalSource();
            }

            // Calculate traditional source weight
            int traditionalWeight = 100 - onlineTxtWeight;

            // Generate random value
            int randomValue = _random.Next(100);

            // If random value is less than TXT weight, use TXT
            if (randomValue < onlineTxtWeight)
            {
                return RhesisDataSource.OnlineTxt;
            }

            // Otherwise use traditional sources
            return GetTraditionalSource();
        }

        private RhesisDataSource GetTraditionalSource()
        {
            int randomValue = _random.Next(4);
            return randomValue switch
            {
                0 => RhesisDataSource.Jinrishici,  // 今日诗词接口概率较低
                1 => RhesisDataSource.Saint,
                2 => RhesisDataSource.Saint,
                _ => RhesisDataSource.Hitokoto
            };
        }
    }
}

public class RhesisData
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
}

public class OnlineTxtData
{
    public string Sentence { get; set; } = string.Empty;

    public RhesisData ToRhesisData()
    {
        return new RhesisData
        {
            Author = string.Empty,
            Title = string.Empty,
            Content = Sentence,
            Source = "在线TXT文件",
            Catalog = "自定义",
        };
    }

    public static OnlineTxtData Fetch(string? requestUrl = null)
    {
        if (string.IsNullOrEmpty(requestUrl))
        {
            return new OnlineTxtData
            {
                Sentence = "请在设置中配置TXT文件URL"
            };
        }

        try
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Add timeout to quickly detect network issues
            };
            var content = client.GetStringAsync(requestUrl).Result;
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                return new OnlineTxtData
                {
                    Sentence = "TXT文件内容为空，请检查URL"
                };
            }

            var random = new Random();
            var randomLine = lines[random.Next(lines.Length)];

            return new OnlineTxtData
            {
                Sentence = randomLine.Trim()
            };
        }
        catch (Exception ex)
        {
            return new OnlineTxtData
            {
                Sentence = $"获取TXT文件时发生错误：请检查网络连接和URL有效性"
            };
        }
    }
}

public class SainticData
{
    [JsonPropertyName("code")]
    public int StatusCode { get; set; } = -1;

    [JsonPropertyName("data")]
    public SainticRhesisData Data { get; set; } = new SainticRhesisData();

    [JsonPropertyName("msg")]
    public string? Message { get; set; }

    [JsonPropertyName("q")]
    public QueueInfoData QueueInfo { get; set; } = new QueueInfoData();

    public class SainticRhesisData
    {
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

    public class QueueInfoData
    {
        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("catalog")]
        public string Catalog { get; set; } = string.Empty;

        [JsonPropertyName("suffix")]
        public string Suffix { get; set; } = string.Empty;

        [JsonPropertyName("theme")]
        public string Theme { get; set; } = string.Empty;
    }

    public RhesisData ToRhesisData()
    {
        return new RhesisData
        {
            Author = Data.Author,
            Title = Data.Name,
            Content = Data.Sentence,
            Source = "诏预API",
            Catalog = $"{Data.Theme}-{Data.Catalog}",
        };
    }

    public static SainticData Fetch(string? requestUrl = null)
    {
        try
        {
            requestUrl ??= "https://open.saintic.com/api/sentence/all.json";
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Add timeout to quickly detect network issues
            };
            return client.GetFromJsonAsync<SainticData>(requestUrl).Result!;
        }
        catch (Exception ex)
        {
            return new SainticData
            {
                Data = new SainticRhesisData
                {
                    Sentence = $"获取时发生错误"
                }
            };
        }
    }
}

public class JinrishiciData
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    public RhesisData ToRhesisData()
    {
        return new RhesisData
        {
            Author = Author,
            Title = Origin,
            Content = Content,
            Source = "今日诗词API",
            Catalog = Category,
        };
    }

    public static JinrishiciData Fetch(int lengthLimitation = 0)
    {
        try
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Add timeout to quickly detect network issues
            };
            return client.GetFromJsonAsync<JinrishiciData>("https://v1.jinrishici.com/all.json").Result!;
        }
        catch (Exception ex)
        {
            return new JinrishiciData
            {
                Content = "获取时发生错误"
            };
        }
    }
}

public class HitokotoData
{
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

    public RhesisData ToRhesisData()
    {
        return new RhesisData
        {
            Author = FromWho,
            Title = From,
            Content = Hitokoto,
            Source = "一言API",
            Catalog = ConvertTypeToString(Type),
        };
    }

    static string ConvertTypeToString(string type)
    {
        string result = type switch
        {
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

    public static HitokotoData Fetch(string? requestUrl = null)
    {
        requestUrl ??= "https://v1.hitokoto.cn/";
        if (RhesisHandler.HitokotoLimitation)
        {
            return new HitokotoData
            {
                Hitokoto = "已达到一言接口限制"
            };
        }
        RhesisHandler.HitokotoLimitation = true;
        new Thread(() => {
            Thread.Sleep(700);
            RhesisHandler.HitokotoLimitation = false;
        }).Start();
        try
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Add timeout to quickly detect network issues
            };
            return client.GetFromJsonAsync<HitokotoData>(requestUrl).Result!;
        }
        catch (Exception ex)
        {
            return new HitokotoData
            {
                Hitokoto = $"获取时发生错误"
            };
        }
    }
}

public enum RhesisDataSource
{
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
