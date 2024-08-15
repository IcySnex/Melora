using Melora.Plugins.Abstract;
using Melora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Melora.Views;

public sealed partial class HomeView : Page
{
    readonly HomeViewModel viewModel = App.Provider.GetRequiredService<HomeViewModel>();

    public HomeView()
    {
        InitializeComponent();

        viewModel.PluginManager.Subscribe<PlatformSupportPlugin>(
            plugin =>
            {
                Button pluginButton = new()
                {
                    Style = (Style)App.Current.Resources["IconButton"],
                    Content = new PathIcon() { Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), plugin.IconPathData) },
                    Command = viewModel.SearchCommand,
                    CommandParameter = plugin.Name
                };
                ToolTipService.SetToolTip(pluginButton, $"Search on {plugin.Name}");
                SearchButtonsContainer.Children.Insert(SearchButtonsContainer.Children.Count - 1, pluginButton);
            },
            plugin =>
            {
                Button? pluginButton = SearchButtonsContainer.Children.OfType<Button>().FirstOrDefault(item => (string)item.CommandParameter == plugin.Name);
                SearchButtonsContainer.Children.Remove(pluginButton);
            });
    }
}