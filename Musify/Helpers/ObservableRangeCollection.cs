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
        IAsyncEnumerator<T> enumerator = items.GetAsyncEnumerator(cancellationToken);
        try
        {
            CheckReentrancy();

            while (await enumerator.MoveNextAsync(cancellationToken))
            {
                T item = enumerator.Current;

                Items.Add(item);
            }
        }
        finally
        {
            if (enumerator is not null)
                await enumerator.DisposeAsync();

            ForceRefresh();
        }
    }
    public async Task AddRangeAsync(
        IAsyncEnumerable<T> items,
        Action<int, T> callback,
        CancellationToken cancellationToken = default!)
    {
        IAsyncEnumerator<T> enumerator = items.GetAsyncEnumerator(cancellationToken);
        try
        {
            CheckReentrancy();

            int index = -1;
            while (await enumerator.MoveNextAsync(cancellationToken))
            {
                index++;
                T item = enumerator.Current;

                callback.Invoke(index, item);

                Items.Add(item);
            }
        }
        finally
        {
            if (enumerator is not null)
                await enumerator.DisposeAsync();

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