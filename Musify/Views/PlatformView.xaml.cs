using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class PlatformView : Page
{
    readonly PlatformViewModel viewModel;

    public PlatformView(
        PlatformViewModel viewModel)
    {
        InitializeComponent();

        this.Name = viewModel.Plugin.Name;
        this.viewModel = viewModel;

        viewModel.SelectedSearchResults = SearchResultsContainer.SelectedItems;
    }


    void OnPageLoaded(object _, RoutedEventArgs _1)
    {
        if (string.IsNullOrWhiteSpace(App.Parameter))
            return;

        viewModel.Query = App.Parameter;
        viewModel.SearchCommand.Execute(null);

        App.Parameter = null;
    }

    void OnSearchResultsContainerSelectionChanged(object _, SelectionChangedEventArgs _1)
    {
        if (SearchResultsContainer.SelectedItems.Count > 0)
        {
            SearchResultsContainer.Padding = new(48, 116, 48, 138);
            DownloadBar.Opacity = 1;
            DownloadBar.IsHitTestVisible = true;
        }
        else
        {
            SearchResultsContainer.Padding = new(48, 116, 48, 24);
            DownloadBar.Opacity = 0;
            DownloadBar.IsHitTestVisible = false;
        }
    }
}