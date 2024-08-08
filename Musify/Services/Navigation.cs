using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Musify.Plugins.Abstract;
using Musify.ViewModels;
using Musify.Views;

namespace Musify.Services;

public class Navigation
{
    readonly ILogger<Navigation> logger;
    readonly MainView mainView;

    public Navigation(
        ILogger<Navigation> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        mainView.NavigationView.SelectionChanged += (s, e) =>
        {
            if (e.SelectedItemContainer.Tag is PlatformSupportPlugin plugin)
            {
                Navigate(plugin.Name, () =>
                {
                    PlatformViewModel viewModel = App.Provider.GetRequiredService<PlatformViewModel>();
                    viewModel.Plugin = plugin;

                    return new PlatformView(viewModel);
                });
                return;
            }

            Navigate((string)e.SelectedItemContainer.Content);
        };
        mainView.NavigationView.BackRequested += (s, e) => GoBack();
        mainView.BackButton.Click += (s, e) => GoBack();

        logger.LogInformation("[Navigation-.ctor] Navigation has been initialized");
    }


    readonly Dictionary<string, Page> viewsCache = new()
    {
        { "Lyrics", new LyricsView() },
        { "Downloads", new DownloadsView() },
        { "Plugins", new PluginsView() },
        { "Settings", new SettingsView() }
    };
    readonly Stack<Page> viewsHistory = [];


    NavigationViewItem? GetItem(
        string key)
    {
        NavigationViewItem? menuItem = (NavigationViewItem?)mainView.NavigationView.MenuItems.FirstOrDefault(item => item is NavigationViewItem navItem && (string)navItem.Content == key);
        if (menuItem is not null)
            return menuItem;

        return (NavigationViewItem?)mainView.NavigationView.FooterMenuItems.FirstOrDefault(item => item is NavigationViewItem navItem && (string)navItem.Content == key);
    }


    void SetPage(
        Page page)
    {
        mainView.Presenter.Content = page;
        viewsHistory.Push(page);
        CanGoBackChanged();

        logger.LogInformation("[Navigation-SetPage] Page was set");
    }


    public void Navigate(
        string key,
        Func<Page> createPage)
    {
        if (!viewsCache.TryGetValue(key, out Page? page))
        {
            page = createPage.Invoke();
            viewsCache[key] = page;
        }

        SetPage(page);
    }

    public void Navigate(
        string key)
    {
        if (viewsCache.TryGetValue(key, out Page? page))
        {
            SetPage(page);
            return;
        }

        Type? pageType = Type.GetType($"Musify.Views.{key}View, Musify");
        if (pageType is null)
        {
            logger.LogError(new Exception($"Returned page type is null. Could not find specified page \"Musify.Views.{key}View, Musify\""), "[Navigation-Navigate] Failed to navigate to page: Could not find page: {key}", key);
            return;
        }
        Page? newPage = (Page?)Activator.CreateInstance(pageType);
        if (newPage is null)
        {
            logger.LogError(new Exception($"Activator could not create an instance of page \"Musify.Views.{key}View, Musify\""), "[Navigation-Navigate] Failed to navigate to page: Could not create page: {key}", key);
            return;
        }

        viewsCache[key] = newPage;
        SetPage(newPage);
    }


    public void SetCurrentItem(
        string key)
    {
        NavigationViewItem? item = GetItem(key);
        if (item is null)
        {
            logger.LogError(new Exception($"There is no navigation view item with the name \"{key}\" in the current menu items."), "[Navigation-SetCurrentIndex] Failed to set current navigation item: Could not find item: {key}", key);
            return;
        }

        mainView.NavigationView.SelectedItem = item;
        logger.LogInformation("[Navigation-SetCurrentItem] Current navigation item was set");
    }


    public void GoBack()
    {
        if (viewsHistory.Count < 2)
        {
            logger.LogError(new Exception("The view history is under 2. Going back is not possible at the time."), "[Navigation-GoBack] Failed to go back: Not possible at the time");
            return;
        }

        viewsHistory.Pop();
        Page page = viewsHistory.Pop();

        SetCurrentItem(page.Name);
    }

    void CanGoBackChanged()
    {
        if (viewsHistory.Count > 1 == (mainView.BackButton.Opacity != 0))
            return;

        if (viewsHistory.Count < 2)
        {
            mainView.BackButton.Opacity = 0;
            ((Storyboard)mainView.BackButton.Resources["OutBoard"]).Begin();
        }
        else
        {
            mainView.BackButton.Opacity = 1;
            ((Storyboard)mainView.BackButton.Resources["InBoard"]).Begin();
        }
    }
}