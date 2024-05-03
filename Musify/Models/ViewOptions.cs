using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Enums;

namespace Musify.Models;

public partial class ViewOptions(
    Sorting sorting,
    bool descending,
    int limit) : ObservableObject
{
    [ObservableProperty]
    Sorting sorting = sorting;

    [ObservableProperty]
    bool descending = descending;

    [ObservableProperty]
    int limit = limit;
}