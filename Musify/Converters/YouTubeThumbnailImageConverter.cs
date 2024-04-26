using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using YoutubeExplode.Common;

namespace Musify.Converters;

public class YouTubeThumbnailImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        IReadOnlyList<Thumbnail> thumbnails = (IReadOnlyList<Thumbnail>)value;

        return new BitmapImage(new(thumbnails.OrderBy(thumbnail => thumbnail.Resolution.Area).FirstOrDefault()?.Url ?? "ms-appx:///na.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}