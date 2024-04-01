using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Aniflex.Core;

public class ObservableList<T> : IList<T>, INotifyCollectionChanged
{
    private readonly List<T> items = new();
    private readonly List<Action<T>> onAdd = new();
    private readonly List<Action<T>> onRemove = new();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void OnItemAdded(T item)
    {
        onAdd.ForEach(action => action.Invoke(item));
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    private void OnItemRemoved(T item, int index)
    {
        onRemove.ForEach(action => action.Invoke(item));
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)items).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)items).Insert(index, item);
        OnItemAdded(item);
    }

    public void RemoveAt(int index)
    {
        T? item = items[index];
        ((IList<T>)items).RemoveAt(index);
        OnItemRemoved(item, index);
    }

    public T this[int index] { get => ((IList<T>)items)[index]; set => ((IList<T>)items)[index] = value; }

    public void Add(T item)
    {
        ((ICollection<T>)items).Add(item);
        OnItemAdded(item);
    }

    public void Clear()
    {
        while (items.Any())
        {
            RemoveAt(items.Count - 1);
        }
    }

    public bool Contains(T item)
    {
        return ((ICollection<T>)items).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)items).CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index <= -1)
            return false;
        RemoveAt(index);
        return true;
    }

    public int Count => ((ICollection<T>)items).Count;

    public bool IsReadOnly => ((ICollection<T>)items).IsReadOnly;

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)items).GetEnumerator();
    }

    public void WhenAdded(Action<T> action)
    {
        items.ForEach(item => action(item));
        onAdd.Add(action);
    }
    public void WhenRemoved(Action<T> action)
    {
        onRemove.Add(action);
    }
}
