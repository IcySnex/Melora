using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using YouTubeMusicAPI.Models;

namespace Musify.Converters;

public class YouTubeMusicThumbnailImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        Thumbnail[] thumbnails = (Thumbnail[])value;

        return new BitmapImage(new(thumbnails.OrderBy(thumbnail => thumbnail.Width * thumbnail.Height).FirstOrDefault()?.Url ?? "ms-appx:///na.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}