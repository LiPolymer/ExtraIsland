namespace ExtraIsland.Shared;

public interface ILyricsProvider {
    event Action? OnLyricsChanged;
    string Lyrics { get; }
    string SubLyrics { get; }
}