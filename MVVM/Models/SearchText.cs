using System.ComponentModel;

namespace Aniflex.MVVM.Models;

public sealed partial class SearchText : INotifyPropertyChanged
{
    public string Text { get; set; } = string.Empty;
}
