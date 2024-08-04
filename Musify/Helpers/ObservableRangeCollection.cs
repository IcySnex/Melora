using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Musify.Helpers;

public class ObservableRangeCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    // Classes
    private sealed class SimpleMonitor : IDisposable
    {
        internal int _busyCount;

        [NonSerialized]
        internal ObservableRangeCollection<T> _collection;

        public SimpleMonitor(ObservableRangeCollection<T> collection)
        {
            Debug.Assert(collection != null);
            _collection = collection;
        }

        public void Dispose() =>
            _collection._blockReentrancyCount--;
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }


    // Reentrancy
    private int _blockReentrancyCount;

    protected IDisposable BlockReentrancy()
    {
        _blockReentrancyCount++;
        return EnsureMonitorInitialized();
    }

    protected void CheckReentrancy()
    {
        if (_blockReentrancyCount <= 0 || !(CollectionChanged?.GetInvocationList().Length > 1))
            return;

        throw new InvalidOperationException("SR.ObservableCollectionReentrancyNotAllowed");
    }


    // Monitor
    private SimpleMonitor? _monitor;

    private SimpleMonitor EnsureMonitorInitialized() =>
        _monitor ??= new SimpleMonitor(this);


    // Constructors
    public ObservableRangeCollection() { }

    public ObservableRangeCollection(
        IEnumerable<T> collection) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection)))) { }

    public ObservableRangeCollection(
        List<T> list) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list)))) { }


    // INotify
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(
        PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }


    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected virtual void OnCollectionChanged(
        NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionChangedEventHandler? handler = CollectionChanged;
        if (handler is null)
            return;

        _blockReentrancyCount++;
        try
        {
            handler(this, e);
        }
        finally
        {
            _blockReentrancyCount--;
        }
    }


    // Serialization
    [OnSerializing]
    private void OnSerializing(
        StreamingContext context)
    {
        EnsureMonitorInitialized();
        _monitor!._busyCount = _blockReentrancyCount;
    }

    [OnDeserialized]
    private void OnDeserialized(
        StreamingContext context)
    {
        if (_monitor is null)
            return;

        _blockReentrancyCount = _monitor._busyCount;
        _monitor._collection = this;
    }


    // Methods
    protected virtual void MoveItem(
        int oldIndex,
        int newIndex)
    {
        CheckReentrancy();
        T removedItem = this[oldIndex];

        originalList.RemoveAt(oldIndex);
        originalList.Insert(newIndex, removedItem);

        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, removedItem);

        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(new(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex));
    }

    protected override void SetItem(
        int index,
        T item)
    {
        CheckReentrancy();
        T originalItem = this[index];

        originalList[index] = item;

        base.SetItem(index, item);

        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, originalItem, item, index));
    }

    protected override void InsertItem(
        int index,
        T item)
    {
        CheckReentrancy();

        originalList.Insert(index, item);

        base.InsertItem(index, item);

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(new(NotifyCollectionChangedAction.Add, item, index));
    }

    protected override void RemoveItem(
        int index)
    {
        CheckReentrancy();
        T removedItem = this[index];

        originalList.RemoveAt(index);

        base.RemoveItem(index);

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, removedItem, index));
    }

    protected override void ClearItems()
    {
        CheckReentrancy();

        originalList.Clear();

        base.ClearItems();

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }


    // Additonal Methods
    public void AddRange(
        IEnumerable<T> items)
    {
        CheckReentrancy();

        using IEnumerator<T> enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
            originalList.Add(enumerator.Current);

        Refresh();

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    public void Refresh()
    {
        if (originalList.Count <= 0)
            return;

        void Add(T item)
        {
            if (Filter is not null && !Filter.Invoke(item))
                return;

            Items.Add(item);
        }

        CheckReentrancy();

        Items.Clear();
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        if (KeySelector is null)
        {
            if (Descending)
                for (int i = originalList.Count - 1; i >= 0 && i >= 0; i--)
                    Add(originalList[i]);
            else
                for (int i = 0; i < originalList.Count; i++)
                    Add(originalList[i]);
        }
        else
        {
            List<T> sortedList = new(Descending ? originalList.OrderByDescending(KeySelector) : originalList.OrderBy(KeySelector));
            for (int i = 0; i < originalList.Count; i++)
                Add(sortedList[i]);
        }

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }


    // Sorting
    private readonly IList<T> originalList = [];


    private Func<T, object>? keySelector = null;

    public Func<T, object>? KeySelector
    {
        get => keySelector;
        set
        {
            keySelector = value;

            Refresh();
        }
    }


    private bool descending = false;

    public bool Descending
    {
        get => descending;
        set
        {
            descending = value;

            Refresh();
        }
    }


    // Filter
    private Func<T, bool>? filter;

    public Func<T, bool>? Filter
    {
        get => filter;
        set
        {
            filter = value;

            Refresh();
        }
    }
}