using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using SpotifyAPI.Web;

namespace Musify.Converters;

public class SpotifyAlbumImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        SimpleAlbum album = (SimpleAlbum)value;

        return new BitmapImage(new(album.Images.FirstOrDefault()?.Url ?? "ms-appx:///na.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}