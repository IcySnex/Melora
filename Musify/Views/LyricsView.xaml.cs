using Microsoft.Extensions.DependencyInjection;
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
}