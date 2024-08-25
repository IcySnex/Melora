using Melora.Models;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Views;

public sealed partial class UpdateInfoView : Page
{
    public Release NewRelease { get; }

    public UpdateInfoView(
        Release newRelease)
    {
        InitializeComponent();

        NewRelease = newRelease;
    }
}