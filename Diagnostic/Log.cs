using System;
using System.Windows;

namespace Aniflex.Diagnostic;

public sealed class Log
{
    public static Log Default { get; } = new();

    public event Action<LogMessage>? OnMessage;

    public void Write(string message)
    {
        OnMessage?.Invoke(new LogMessage(DateTime.Now, message));
    }

    public void Exception(Exception exception)
    {
        OnMessage?.Invoke(new LogMessage(DateTime.Now, exception.Message, IsException: true));
    }
}
