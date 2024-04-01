using Aniflex.Diagnostic;

using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Aniflex.MVVM.ViewModels;

public sealed class LogViewModel
{
    public ObservableCollection<LogMessage> Messages { get; set; } = new();

    private readonly Dispatcher dispatcher;

    public LogViewModel()
    {
        Log.Default.OnMessage += OnLogMessage;
        dispatcher = Dispatcher.CurrentDispatcher;
    }

    private void OnLogMessage(LogMessage obj)
    {
        dispatcher.Invoke(() =>
        {
            Messages.Add(obj);
        }, DispatcherPriority.Background);
    }
}
