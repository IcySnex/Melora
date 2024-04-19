using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class LyricsView : Page
{
    readonly LyricsViewModel viewModel = App.Provider.GetRequiredService<LyricsViewModel>();

    public LyricsView()
    {
        InitializeComponent();
    }


    protected override void OnNavigatedTo(
        NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not string parameter)
            return;

        viewModel.Query = parameter;
        viewModel.SearchCommand.Execute(null);
    }
}