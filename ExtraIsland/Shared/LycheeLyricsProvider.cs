using LycheeLib.Interface;

namespace ExtraIsland.Shared;

public class LycheeLyricsProvider: ILyricsProvider {
    public LycheeLyricsProvider() {
        Rendezvous.OnLyricsChanged += lyrics => {
            Lyrics = lyrics[0];
            SubLyrics = lyrics[1];
            OnLyricsChanged?.Invoke();
        };
    }
    
    public event Action? OnLyricsChanged;
    public string Lyrics { get; private set; } = string.Empty;
    public string SubLyrics { get; private set; } = string.Empty;
}