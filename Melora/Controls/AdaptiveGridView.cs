// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Source: https://github.com/CommunityToolkit/WindowsCommunityToolkit

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Melora.Converters;
using System.Windows.Input;
using Windows.Foundation.Collections;

namespace Melora.Controls;

/// <summary>
/// The AdaptiveGridView control allows to present information within a Grid View perfectly adjusting the
/// total display available space. It reacts to changes in the layout as well as the content so it can adapt
/// to different form factors automatically.
/// </summary>
/// <remarks>
/// The number and the width of items are calculated based on the
/// screen resolution in order to fully leverage the available screen space. The property ItemsHeight define
/// the items fixed height and the property DesiredWidth sets the minimum width for the elements to add a
/// new column.</remarks>
public partial class AdaptiveGridView : GridView
{
    /// <summary>
    /// Identifies the <see cref="ItemClickCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemClickCommandProperty =
        DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(AdaptiveGridView), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="ItemHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(AdaptiveGridView), new PropertyMetadata(double.NaN));

    /// <summary>
    /// Identifies the <see cref="OneRowModeEnabled"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OneRowModeEnabledProperty =
        DependencyProperty.Register(nameof(OneRowModeEnabled), typeof(bool), typeof(AdaptiveGridView), new PropertyMetadata(false, (o, e) => { OnOneRowModeEnabledChanged(o, e.NewValue); }));

    /// <summary>
    /// Identifies the <see cref="ItemWidth"/> dependency property.
    /// </summary>
    private static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(AdaptiveGridView), new PropertyMetadata(double.NaN));

    /// <summary>
    /// Identifies the <see cref="DesiredWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DesiredWidthProperty =
        DependencyProperty.Register(nameof(DesiredWidth), typeof(double), typeof(AdaptiveGridView), new PropertyMetadata(double.NaN, DesiredWidthChanged));

    /// <summary>
    /// Identifies the <see cref="StretchContentForSingleRow"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StretchContentForSingleRowProperty =
    DependencyProperty.Register(nameof(StretchContentForSingleRow), typeof(bool), typeof(AdaptiveGridView), new PropertyMetadata(true, OnStretchContentForSingleRowPropertyChanged));

    private static void OnOneRowModeEnabledChanged(DependencyObject d, object _)
    {
        AdaptiveGridView? self = d as AdaptiveGridView;
        self?.DetermineOneRowMode();
    }

    private static void DesiredWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AdaptiveGridView? self = d as AdaptiveGridView;
        self?.RecalculateLayout(self.ActualWidth);
    }

    private static void OnStretchContentForSingleRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AdaptiveGridView? self = d as AdaptiveGridView;
        self?.RecalculateLayout(self.ActualWidth);
    }

    /// <summary>
    /// Gets or sets the desired width of each item
    /// </summary>
    /// <value>The width of the desired.</value>
    public double DesiredWidth
    {
        get => (double)GetValue(DesiredWidthProperty);
        set => SetValue(DesiredWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control should stretch the content to fill at least one row.
    /// </summary>
    /// <remarks>
    /// If set to <c>true</c> (default) and there is only one row of items, the items will be stretched to fill the complete row.
    /// If set to <c>false</c>, items will have their normal size, which means a gap can exist at the end of the row.
    /// </remarks>
    /// <value>A value indicating whether the control should stretch the content to fill at least one row.</value>
    public bool StretchContentForSingleRow
    {
        get => (bool)GetValue(StretchContentForSingleRowProperty);
        set => SetValue(StretchContentForSingleRowProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is clicked and the IsItemClickEnabled property is true.
    /// </summary>
    /// <value>The item click command.</value>
    public ICommand ItemClickCommand
    {
        get => (ICommand)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of each item in the grid.
    /// </summary>
    /// <value>The height of the item.</value>
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether only one row should be displayed.
    /// </summary>
    /// <value><c>true</c> if only one row is displayed; otherwise, <c>false</c>.</value>
    public bool OneRowModeEnabled
    {
        get => (bool)GetValue(OneRowModeEnabledProperty);
        set => SetValue(OneRowModeEnabledProperty, value);
    }

    /// <summary>
    /// Gets the template that defines the panel that controls the layout of items.
    /// </summary>
    /// <remarks>
    /// This property overrides the base ItemsPanel to prevent changing it.
    /// </remarks>
    /// <returns>
    /// An ItemsPanelTemplate that defines the panel to use for the layout of the items.
    /// The default value for the ItemsControl is an ItemsPanelTemplate that specifies
    /// a StackPanel.
    /// </returns>
    public new ItemsPanelTemplate ItemsPanel => base.ItemsPanel;

    private double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    private static int CalculateColumns(double containerWidth, double itemWidth)
    {
        int columns = (int)Math.Round(containerWidth / itemWidth);
        if (columns == 0)
            columns = 1;

        return columns;
    }


    private bool _isLoaded;
    private ScrollMode _savedVerticalScrollMode;
    private ScrollMode _savedHorizontalScrollMode;
    private ScrollBarVisibility _savedVerticalScrollBarVisibility;
    private ScrollBarVisibility _savedHorizontalScrollBarVisibility;
    private Orientation _savedOrientation;
    private bool _needToRestoreScrollStates;
    private bool _needContainerMarginForLayout;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveGridView"/> class.
    /// </summary>
    public AdaptiveGridView()
    {
        IsTabStop = false;
        SizeChanged += OnSizeChanged;
        ItemClick += OnItemClick;
        Items.VectorChanged += ItemsOnVectorChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Prevent issues with higher DPIs and underlying panel. #1803
        UseLayoutRounding = false;
    }

    /// <summary>
    /// Prepares the specified element to display the specified item.
    /// </summary>
    /// <param name="obj">The element that's used to display the specified item.</param>
    /// <param name="item">The item to display.</param>
    protected override void PrepareContainerForItemOverride(DependencyObject obj, object item)
    {
        base.PrepareContainerForItemOverride(obj, item);
        if (obj is FrameworkElement element)
        {
            Binding heightBinding = new()
            {
                Source = this,
                Path = new PropertyPath("ItemHeight"),
                Mode = BindingMode.TwoWay
            };
            Binding widthBinding = new()
            {
                Source = this,
                Path = new PropertyPath("ItemWidth"),
                Mode = BindingMode.TwoWay
            };

            element.SetBinding(HeightProperty, heightBinding);
            element.SetBinding(WidthProperty, widthBinding);
        }

        if (obj is ContentControl contentControl)
        {
            contentControl.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            contentControl.VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        if (_needContainerMarginForLayout)
        {
            _needContainerMarginForLayout = false;
            RecalculateLayout(ActualWidth);
        }
    }

    /// <summary>
    /// Calculates the width of the grid items.
    /// </summary>
    /// <param name="containerWidth">The width of the container control.</param>
    /// <returns>The calculated item width.</returns>
    protected virtual double CalculateItemWidth(double containerWidth)
    {
        if (double.IsNaN(DesiredWidth))
            return DesiredWidth;

        int columns = CalculateColumns(containerWidth, DesiredWidth);

        // If there's less items than there's columns, reduce the column count (if requested);
        if (Items is not null && Items.Count > 0 && Items.Count < columns && StretchContentForSingleRow)
            columns = Items.Count;

        // subtract the margin from the width so we place the correct width for placement
        Thickness fallbackThickness = default;
        Thickness itemMargin = AdaptiveHeightValueConverter.GetItemMargin(this, fallbackThickness);

        if (itemMargin == fallbackThickness)
            _needContainerMarginForLayout = true; // No style explicitly defined, or no items or no container for the items. We need to get an actual margin for proper layout

        return (containerWidth / columns) - itemMargin.Left - itemMargin.Right;
    }

    /// <summary>
    /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call
    /// ApplyTemplate. In simplest terms, this means the method is called just before a UI element displays
    /// in your app. Override this method to influence the default post-template logic of a class.
    /// </summary>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnOneRowModeEnabledChanged(this, OneRowModeEnabled);
    }

    private void ItemsOnVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
    {
        if (double.IsNaN(ActualWidth))
            return;

        // If the item count changes, check if more or less columns needs to be rendered,
        // in case we were having fewer items than columns.
        RecalculateLayout(ActualWidth);
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        ICommand cmd = ItemClickCommand;
        if (cmd is null)
            return;

        if (cmd.CanExecute(e.ClickedItem))
        {
            cmd.Execute(e.ClickedItem);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // If we are in center alignment, we only care about relayout if the number of columns we can display changes
        // Fixes #1737
        if (HorizontalAlignment != HorizontalAlignment.Stretch)
        {
            int prevColumns = CalculateColumns(e.PreviousSize.Width, DesiredWidth);
            int newColumns = CalculateColumns(e.NewSize.Width, DesiredWidth);

            // If the width of the internal list view changes, check if more or less columns needs to be rendered.
            if (prevColumns != newColumns)
                RecalculateLayout(e.NewSize.Width);
        }
        else if (e.PreviousSize.Width != e.NewSize.Width)
        {
            // We need to recalculate width as our size changes to adjust internal items.
            RecalculateLayout(e.NewSize.Width);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        DetermineOneRowMode();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
    }

    private void DetermineOneRowMode()
    {
        if (!_isLoaded)
            return;

        var itemsWrapGridPanel = ItemsPanelRoot as ItemsWrapGrid;

        if (OneRowModeEnabled)
        {
            Binding b = new()
            {
                Source = this,
                Path = new PropertyPath("ItemHeight"),
                Converter = new AdaptiveHeightValueConverter(),
                ConverterParameter = this
            };

            if (itemsWrapGridPanel is not null)
            {
                _savedOrientation = itemsWrapGridPanel.Orientation;
                itemsWrapGridPanel.Orientation = Orientation.Vertical;
            }

            SetBinding(MaxHeightProperty, b);

            _savedHorizontalScrollMode = ScrollViewer.GetHorizontalScrollMode(this);
            _savedVerticalScrollMode = ScrollViewer.GetVerticalScrollMode(this);
            _savedHorizontalScrollBarVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(this);
            _savedVerticalScrollBarVisibility = ScrollViewer.GetVerticalScrollBarVisibility(this);
            _needToRestoreScrollStates = true;

            ScrollViewer.SetVerticalScrollMode(this, ScrollMode.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Visible);
            ScrollViewer.SetHorizontalScrollMode(this, ScrollMode.Enabled);
        }
        else
        {
            ClearValue(MaxHeightProperty);

            if (!_needToRestoreScrollStates)
                return;

            _needToRestoreScrollStates = false;

            if (itemsWrapGridPanel is not null)
                itemsWrapGridPanel.Orientation = _savedOrientation;

            ScrollViewer.SetVerticalScrollMode(this, _savedVerticalScrollMode);
            ScrollViewer.SetVerticalScrollBarVisibility(this, _savedVerticalScrollBarVisibility);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, _savedHorizontalScrollBarVisibility);
            ScrollViewer.SetHorizontalScrollMode(this, _savedHorizontalScrollMode);
        }
    }

    private void RecalculateLayout(double containerWidth)
    {
        Panel itemsPanel = ItemsPanelRoot;
        double panelMargin = itemsPanel is not null ? itemsPanel.Margin.Left + itemsPanel.Margin.Right : 0;

        double padding = Padding.Left + Padding.Right;
        double border = BorderThickness.Left + BorderThickness.Right;

        // width should be the displayable width
        containerWidth = containerWidth - padding - panelMargin - border;
        if (containerWidth > 0)
        {
            double newWidth = CalculateItemWidth(containerWidth);
            ItemWidth = Math.Floor(newWidth);
        }
    }
}