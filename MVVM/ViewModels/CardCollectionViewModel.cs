using Aniflex.Core;
using Aniflex.MVVM.Models;

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace Aniflex.MVVM.ViewModels;

public sealed class CardCollectionViewModel : ObservableList<CardViewModel>
{
    private SearchText? filter;

    public SearchText? Filter
    {
        get => filter;
        set
        {
            if (filter is not null)
            {
                filter.PropertyChanged -= Filter_PropertyChanged;
            }
            filter = value;
            if (filter is not null)
            {
                filter.PropertyChanged += Filter_PropertyChanged;
            }
        }
    }

    private ICollectionView? view;
    public ICollectionView? View
    {
        get
        {
            if (view is null)
            {
                view = CollectionViewSource.GetDefaultView(this);
                view.Filter = (object obj) =>
                {
                    if (obj is not CardViewModel card || card.Anime is null || card.Anime.Name is null || Filter is null)
                        return true;

                    return card.Anime.Name.Contains(Filter.Text, StringComparison.CurrentCultureIgnoreCase);
                };
                view.SortDescriptions.Add(new SortDescription("Anime.IsFavorite", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Anime.IsWatching", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Anime.WatchedEpisodes", ListSortDirection.Descending));
            }
            return view;
        }
    }

    public CardCollectionViewModel()
    {
        CollectionChanged += CardCollectionViewModel_CollectionChanged;
    }

    private void CardCollectionViewModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            foreach (object item in e.OldItems)
            {
                if (item is not CardViewModel card || card.Anime is null)
                    continue;

                card.Anime.PropertyChanged -= Anime_PropertyChanged;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            foreach (object item in e.NewItems)
            {
                if (item is not CardViewModel card || card.Anime is null)
                    continue;

                card.Anime.PropertyChanged += Anime_PropertyChanged;
            }
        }
    }

    private void Anime_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Refresh();
    }

    private void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (view is not null)
            view.Refresh();
    }
}
