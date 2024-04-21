using Microsoft.UI.Xaml.Data;
using SpotifyAPI.Web;

namespace Musify.Converters;

public class SpotifyArtistsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        List<SimpleArtist> artists = (List<SimpleArtist>)value;
        IEnumerable<string> artistNames = artists.Select(artist => artist.Name);

        string result = string.Join(", ", artistNames);
        return string.IsNullOrEmpty(result) ? "N/A" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}