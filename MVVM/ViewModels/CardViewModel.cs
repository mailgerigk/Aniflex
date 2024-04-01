using Aniflex.MVVM.Models;
using Aniflex.MVVM.Windows;

using Prism.Commands;

using PropertyChanged;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class CardViewModel : INotifyPropertyChanged
{
    private readonly Dispatcher dispatcher;

    [OnChangedMethod(nameof(OnAnimeChanged))]
    public Anime? Anime { get; set; }

    public ICommand ToggleFavorite { get; set; }
    public ICommand OpenAnimeWindow { get; set; }

    public BitmapSource? Thumbnail { get; set; }

    private AnimeWindow? window;

    public CardViewModel()
    {
        dispatcher = Dispatcher.CurrentDispatcher;

        ToggleFavorite = new DelegateCommand(() =>
        {
            if (Anime is null)
                return;

            Anime.IsFavorite = !Anime.IsFavorite;
        });
        OpenAnimeWindow = new DelegateCommand(() =>
        {
            if (Anime is null)
                return;

            if (window is not null)
            {
                window.Focus();
                return;
            }

            window = new AnimeWindow()
            {
                DataContext = new AnimeWindowViewModel(new AnimeViewModel(Anime)),
            };
            window.Closing += Window_Closing;
            window.Show();
        });
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (window is not null)
        {
            window.Closing -= Window_Closing;
            window = null;
        }
    }

    private async Task ChangeThumbnail()
    {
        if (Anime is null || Anime.Thumbnail is null)
            return;

        BitmapSource source = await Anime.Thumbnail.LoadSource();
        await dispatcher.BeginInvoke(() =>
        {
            Thumbnail = source;
        }, DispatcherPriority.Background);
    }

    private void OnAnimeChanged()
    {
        Task _ = ChangeThumbnail();
    }
}
