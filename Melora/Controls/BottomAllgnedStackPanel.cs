using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Melora.Controls;

public class BottomAlignedStackPanel : StackPanel
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        Size arrangedSize = base.ArrangeOverride(finalSize);

        double remainingHeight = finalSize.Height;
        double totalChildrenHeight = 0;
        foreach (UIElement child in Children)
        {
            if (child.Visibility == Visibility.Collapsed)
                continue;

            totalChildrenHeight += child.DesiredSize.Height + Spacing;
        }

        double yOffset = remainingHeight - totalChildrenHeight;
        foreach (var child in Children)
        {
            if (child.Visibility == Visibility.Collapsed)
                continue;

            child.Arrange(new Rect(0, yOffset, finalSize.Width, child.DesiredSize.Height));
            yOffset += child.DesiredSize.Height + Spacing;
        }

        return arrangedSize;
    }
}