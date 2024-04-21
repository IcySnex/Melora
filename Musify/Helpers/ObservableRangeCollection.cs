using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Musify.Helpers;

public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public bool SkipForceRefresh { get; set; }

    public void ForceRefresh()
    {
        if (!SkipForceRefresh)
            OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
    }


    public void AddRange(
        IEnumerable<T> items)
    {
        try
        {
            CheckReentrancy();

            foreach (T item in items)
                Items.Add(item);
        }
        finally
        {
            ForceRefresh();
        }
    }

    public async Task AddRangeAsync(
        IAsyncEnumerable<T> items,
        CancellationToken cancellationToken = default!)
    {
        try
        {
            CheckReentrancy();

            await foreach (T item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Items.Add(item);
            }
        }
        finally
        {
            ForceRefresh();
        }
    }


    public new void Add(
        T item)
    {
        try
        {
            Items.Add(item);
        }
        finally
        {
            ForceRefresh();
        }
    }


    public new bool Remove(
        T item)
    {
        try
        {
            return Items.Remove(item);
        }
        finally
        {
            ForceRefresh();
        }
    }

    public new void RemoveAt(
        int index)
    {
        try
        {
            Items.RemoveAt(index);
        }
        finally
        {
            ForceRefresh();
        }
    }

    public void RemoveRange(
        IEnumerable<T> items)
    {
        try
        {
            CheckReentrancy();

            foreach (T item in items)
                Items.Remove(item);
        }
        finally
        {
            ForceRefresh();
        }
    }
}