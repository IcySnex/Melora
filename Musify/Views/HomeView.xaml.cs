using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Musify.Plugins.Abstract;
using Musify.Services;
using Musify.ViewModels;

namespace Musify.Views;

public sealed partial class HomeView : Page
{
    readonly HomeViewModel viewModel = App.Provider.GetRequiredService<HomeViewModel>();

    public HomeView()
    {
        InitializeComponent();

        App.Provider.GetRequiredService<PluginManager<PlatformSupportPlugin>>().LoadedPlugins.CollectionChanged += (s, e) =>
        {
            if (e.NewItems is not null)
                foreach (PlatformSupportPlugin plugin in e.NewItems.Cast<PlatformSupportPlugin>())
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
                }

            if (e.OldItems is not null)
                foreach (PlatformSupportPlugin plugin in e.OldItems.Cast<PlatformSupportPlugin>())
                {
                    Button? pluginButton = SearchButtonsContainer.Children.OfType<Button>().FirstOrDefault(item => (string)item.CommandParameter == plugin.Name);
                    SearchButtonsContainer.Children.Remove(pluginButton);
                }
        };
    }
}