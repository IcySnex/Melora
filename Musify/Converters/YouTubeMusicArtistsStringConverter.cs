using Microsoft.UI.Xaml.Data;
using YouTubeMusicAPI.Models.Shelf;

namespace Musify.Converters;

public class YouTubeMusicArtistsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        ShelfItem[] artists = (ShelfItem[])value;
        IEnumerable<string> artistNames = artists.Select(artist => artist.Name);

        string result = string.Join(", ", artistNames);
        return string.IsNullOrEmpty(result) ? "N/A" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}