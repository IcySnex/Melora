using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class LyricsView : Page
{
    readonly LyricsViewModel viewModel = App.Provider.GetRequiredService<LyricsViewModel>();

    public LyricsView()
    {
        InitializeComponent();
    }


    void OnPageLoaded(object _, RoutedEventArgs _1)
    {
        if (string.IsNullOrWhiteSpace(App.Parameter))
            return;

        viewModel.Query = App.Parameter;
        viewModel.SearchCommand.Execute(null);

        App.Parameter = null;
    }
}