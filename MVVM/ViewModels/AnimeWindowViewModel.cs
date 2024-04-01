using System.ComponentModel;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class AnimeWindowViewModel : INotifyPropertyChanged
{
    public AnimeViewModel AnimeViewModel { get; set; }

    public AnimeWindowViewModel(AnimeViewModel animeViewModel)
    {
        AnimeViewModel = animeViewModel;
    }
}
