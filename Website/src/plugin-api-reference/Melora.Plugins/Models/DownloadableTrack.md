# DownloadableTrack
Represents a track that can be downloaded\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging)
```cs
public class DownloadableTrack : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging
```


## Constructors
Creates a new DownloadableTrack\.
```cs
public DownloadableTrack(
  string title, 
  string artists, 
  TimeSpan duration, 
  string artworkUrl, 
  bool isExplicit, 
  DateTime releasedAt, 
  string album, 
  string genre, 
  string lyrics, 
  int trackNumber, 
  int totalTracks, 
  string copyright, 
  string comment, 
  string url, 
  string id)
```
| Parameter | Summary |
| --------- | ------- |
| `string` title | The title of the downloadable track. |
| `string` artists | The artists of the downloadable track. |
| `TimeSpan` duration | The duration of the downloadable track. |
| `string` artworkUrl | The url to the artwork of the downloadable track. |
| `bool` isExplicit | Whether the downloadable track is explicit or not. |
| `DateTime` releasedAt | The date and time when the downloadable track was released. |
| `string` album | The album of the downloadable track. |
| `string` genre | The genre of the downloadable track. |
| `string` lyrics | The lyrics of the downloadable track. |
| `int` trackNumber | The track number of the downloadable track. |
| `int` totalTracks | The total tracks of the downloadable tracks album. |
| `string` copyright | The copyright of the downloadable tracks. |
| `string` comment | An optional comment from the plugin. |
| `string` url | The url of the downloadable track. |
| `string` id | The Id of the downloadable track. |





## Properties

### Duration
The duration of the downloadable track\.
- **Type:** [System.TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
- **Is Read Only:** `True`

### Comment
An optional comment from the plugin\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Url
The url of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Id
The Id of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Title
The title of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### Artists
The artists of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### ArtworkUrl
The url to the artwork of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### IsExplicit
Whether the downloadable track is explicit or not\.
- **Type:** [System.Boolean](https://learn.microsoft.com/dotnet/api/system.bool)
- **Is Read Only:** `False`

### ReleasedAt
The date and time when the downloadable track was released\.
- **Type:** [System.DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)
- **Is Read Only:** `False`

### Album
The album of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### Genre
The genre of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### Lyrics
The lyrics of the downloadable track\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### TrackNumber
The track number of the downloadable track\.
- **Type:** [System.Int32](https://learn.microsoft.com/dotnet/api/system.int)
- **Is Read Only:** `False`

### TotalTracks
The total tracks of the downloadable track's album\.
- **Type:** [System.Int32](https://learn.microsoft.com/dotnet/api/system.int)
- **Is Read Only:** `False`

### Copyright
The copyright of the downloadable track's album\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`
