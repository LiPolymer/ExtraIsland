namespace ExtraIsland.Shared;

public interface ITerminalProvider {
    void Write(string content);
    void WriteLine(string content);
}

public static class Terminal {
    static readonly ITerminalProvider? Provider = new LegacyTerminal();
    
    public static void WriteLine(string unit, string content, MessageType type = MessageType.Info) {
        #if !DEBUG
        if (type == MessageType.Debug){
            return;
        }
        #endif
        string prefix = type switch {
            MessageType.Info => "&2INFO",
            MessageType.None => string.Empty,
            MessageType.Warn => "&cWARN",
            MessageType.Error => "&4ERRO",
            MessageType.Critical => "&4FAIL",
            // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
            MessageType.Debug => "&9DeBG",
            _ => throw new ArgumentOutOfRangeException(nameof(type),type,null)
        };
        WriteLine($"&8[{prefix}&8]&7{unit}&r&8>&r{content}");
    }
    public static void Write(string content) {
        Provider?.Write(content);
    }
    public static void WriteLine(string content) {
        Provider?.WriteLine(content);
    }
    public enum MessageType {
        None,
        Info,
        Warn,
        Error,
        Critical,
        Debug
    }
}

public class LegacyTerminal : ITerminalProvider {
    readonly ConsoleColor _defaultForeground = Console.ForegroundColor;
    static List<Tuple<string,string>> Format(string input) {
        List<Tuple<string,string>> result = [];
        if (string.IsNullOrEmpty(input))
            return result;
        int firstAmpIndex = input.IndexOf('&');
        if (firstAmpIndex == -1) {
            result.Add(Tuple.Create("r",input));
            return result;
        }
        string firstPart = input[..firstAmpIndex];
        result.Add(Tuple.Create("r",firstPart));
        string remaining = input[firstAmpIndex..];
        while (remaining.Length >= 2) {
            char key = remaining[1];
            string nextPart = remaining[2..];

            int nextAmpIndex = nextPart.IndexOf('&');
            if (nextAmpIndex == -1) {
                result.Add(Tuple.Create(key.ToString(),nextPart));
                break;
            }
            string value = nextPart[..nextAmpIndex];
            result.Add(Tuple.Create(key.ToString(),value));
            remaining = nextPart[nextAmpIndex..];
        }
        return result;
    }
    static ConsoleColor? McColorToConsoleColor(string minecraftColor) {
        return minecraftColor switch {
            "0" => ConsoleColor.Black,
            "1" => ConsoleColor.DarkBlue,
            "2" => ConsoleColor.DarkGreen,
            "3" => ConsoleColor.DarkCyan,
            "4" => ConsoleColor.DarkRed,
            "5" => ConsoleColor.DarkMagenta,
            "6" => ConsoleColor.DarkYellow,
            "7" => ConsoleColor.Gray,
            "8" => ConsoleColor.DarkGray,
            "9" => ConsoleColor.Blue,
            "a" => ConsoleColor.Green,
            "b" => ConsoleColor.Cyan,
            "c" => ConsoleColor.Red,
            "d" => ConsoleColor.Magenta,
            "e" => ConsoleColor.Yellow,
            "f" => ConsoleColor.White,
            "g" => ConsoleColor.DarkYellow,
            _ => null
        };
    }
    
    public void Write(string content) {
        List<Tuple<string,string>> parts = Format(content);
        foreach (Tuple<string,string> part in parts) {
            ConsoleColor? color = part.Item1 == "r" ? _defaultForeground : McColorToConsoleColor(part.Item1);
            if (color != null) {
                Console.ForegroundColor = color.Value;
            }
            Console.Write(part.Item2);
        }
    }

    public void WriteLine(string content) {
        ConsoleColor defaultColor = Console.ForegroundColor;
        Write(content);
        Console.ForegroundColor = defaultColor;
        Console.WriteLine();
    }
}

public class ChainedTerminal {
    readonly List<string> _unitChain;
    public ChainedTerminal(string firstNode) {
        _unitChain = [firstNode];
    }
    public ChainedTerminal(List<string> chain) {
        _unitChain = chain;
    }

    public ChainedTerminal Chain(string node) {
        List<string> newChain = [];
        _unitChain.ForEach(n => newChain.Add(n));
        newChain.Add(node);
        return new ChainedTerminal(newChain);
    }

    public void AddNode(string node) {
        _unitChain.Add(node);
    }

    public void WriteLine(string content, Terminal.MessageType type = Terminal.MessageType.Info) {
        string prefix = string.Empty;
        _unitChain.ForEach(n => prefix += n + "&8>&r");
        Terminal.WriteLine(prefix, content, type);
    }
}