using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Musify.Views;
using System;

namespace Musify.Services;

public class Navigation
{
    readonly ILogger<Navigation> logger;
    readonly MainView mainView;

    bool skipEvent = false;

    public Navigation(
        ILogger<Navigation> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        mainView.NavigationView.SelectionChanged += (s, e) =>
        {
            if (!skipEvent && e.SelectedItemContainer is NavigationViewItem item)
                Navigate(item.Content.ToString()!);
        };
        mainView.NavigationView.BackRequested += (s, e) => GoBack();
        mainView.BackButton.Click += (s, e) => GoBack();

        logger.LogInformation("[Navigation-.ctor] Navigation has been initialized");
    }


    public void Navigate(
        string page,
        object? parameter = null)
    {
        Type? pageType = Type.GetType($"Musify.Views.{page.Replace(" ", string.Empty)}View, Musify");
        if (pageType is null)
        {
            logger.LogError("[Navigation-Navigate] Failed to navigate: Could not find page: {page}", page);
            return;
        }

        mainView.ContentFrame.Navigate(pageType, parameter);
        CanGoBackChanged();

        logger.LogInformation("[Navigation-Navigate] Navigated to page: {page}", page);
    }

    public bool SetCurrentIndex(
        int index)
    {
        try
        {
            object selectedItem = mainView.NavigationView.MenuItems.ElementAt(index);

            if (mainView.NavigationView.SelectedItem == selectedItem)
                return false;

            skipEvent = true;
            mainView.NavigationView.SelectedItem = selectedItem;
            skipEvent = false;

            logger.LogInformation("[Navigation-SetCurrentIndex] Set current navigation item");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("[Navigation-SetCurrentIndex] Failed to set current navigation item: {error}", ex.Message);
            return false;
        }
    }


    public void GoBack()
    {
        if (!mainView.ContentFrame.CanGoBack)
        {
            logger.LogError("[Navigation-GoBack] Failed to go back: Not possible at the time");
            return;
        }

        mainView.ContentFrame.GoBack();
        CanGoBackChanged();

        object? selectedItem = mainView.NavigationView.MenuItems.Where(item => item is NavigationViewItem navItem && Type.GetType($"Musify.Views.{navItem.Content.ToString()?.Replace(" ", string.Empty)}View, Musify") == mainView.ContentFrame.CurrentSourcePageType).FirstOrDefault();
        if (selectedItem is null)
        {
            logger.LogError("[Navigation-GoBack] Failed to set current navigation item: selectedItem is null");
        }
        
        skipEvent = true;
        mainView.NavigationView.SelectedItem = selectedItem;
        skipEvent = false;

        logger.LogInformation("[Navigation-GoBack] Navigated one page back");
    }


    void CanGoBackChanged()
    {
        if (mainView.ContentFrame.CanGoBack == (mainView.BackButton.Opacity != 0))
            return;

        if (mainView.ContentFrame.CanGoBack)
        {
            mainView.BackButton.Opacity = 1;
            ((Storyboard)mainView.BackButton.Resources["InBoard"]).Begin();
        }
        else
        {
            mainView.BackButton.Opacity = 0;
            ((Storyboard)mainView.BackButton.Resources["OutBoard"]).Begin();
        }
    }
}