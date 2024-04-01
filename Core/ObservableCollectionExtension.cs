using System;
using System.Collections.ObjectModel;

namespace Aniflex.Core;

public static class ObservableCollectionExtension
{
    public static void ForEach<T>(this ObservableCollection<T> vs, Action<T> action)
    {
        foreach (T? v in vs)
        {
            action(v);
        }
    }
}
