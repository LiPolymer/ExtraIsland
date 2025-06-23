using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace ExtraIsland.Shared;

public class LyricsIslandHandler : IDisposable, ILyricsProvider {

    public LyricsIslandHandler(string url = "http://127.0.0.1:50063/") {
        Url = url;
        _listener = new HttpListener();
        _listener.Prefixes.Add(Url);
        StartHttpListener();
    }

    readonly HttpClient _httpClient = new HttpClient();
    readonly HttpListener _listener;
    string Url { get; }
    bool _isListening;

    public string LogMessage { get; set; } = string.Empty;
    public bool Status { get; private set; } = true;
    public string Lyrics { get; private set; } = string.Empty;
    public string SubLyrics { get; private set; } = string.Empty;
    public event Action? OnLyricsChanged;

    void StartHttpListener() {
        try {
            _listener.Start();
            _isListening = true;
            ListenAsync();
            LogMessage = "启动成功";
        }
        catch (HttpListenerException ex) {
            Status = false;
            Console.WriteLine($"[ExIsLand][Tracer][LyricsIslandHandler] 启动 HTTP 监听器失败: {ex.Message}");
            LogMessage = $"启动失败 {ex.Message}";
        }
    }

    async void ListenAsync() {
        while (_isListening && _listener.IsListening) {
            try {
                HttpListenerContext context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995) {
                // 操作已中止。
                // 监听器已停止，无需处理。
            }
            catch (Exception ex) {
                Console.WriteLine($"[ExIsLand][Tracer][LyricsIslandHandler] 监听过程中发生错误: {ex.Message}");
                LogMessage = $"解析错误 {ex.Message}";
            }
        }
    }

    async Task HandleRequestAsync(HttpListenerContext context) {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        if (request is { 
                HttpMethod: "POST",
                Url.LocalPath: "/component/lyrics/lyrics/" }) {
            try {
                using (StreamReader reader = new StreamReader(request.InputStream,request.ContentEncoding)) {
                    string json = await reader.ReadToEndAsync();
                    ParseLyricFromJson(json);
                    OnLyricsChanged?.Invoke();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "text/plain";
                await using (StreamWriter writer = new StreamWriter(response.OutputStream)) {
                    await writer.WriteAsync("歌词更新成功！");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"处理请求时出错: {ex.Message}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await using StreamWriter writer = new StreamWriter(response.OutputStream);
                await writer.WriteAsync("内部服务器错误");
            }
        } else {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            await using StreamWriter writer = new StreamWriter(response.OutputStream);
            await writer.WriteAsync("未找到请求的资源");
        }

        response.Close();
    }

    void ParseLyricFromJson(string json) {
        // 示例 JSON: { "lyric": "你的歌词"}
        try {
            JsonDocument jsonDoc = JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("lyric",out JsonElement lyricElement)) {
                Lyrics = lyricElement.GetString() ?? "未解析到歌词";
            }
            if (jsonDoc.RootElement.TryGetProperty("extra",out JsonElement subLyricElement)) {
                SubLyrics = subLyricElement.GetString() ?? "未解析到次级歌词";
            }
        }
        catch (JsonException) { }
    }

    public void Dispose() {
        _isListening = false;
        if (_listener.IsListening) {
            _listener.Stop();
            _listener.Close();
        }
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}