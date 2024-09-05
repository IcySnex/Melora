# DoubleOption
Represents a custom double option for a plugin config\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging), [IOption](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IOption.html)
```cs
public class DoubleOption : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, IOption
```


## Constructors
Creates a new DoubleOption\.
```cs
public DoubleOption(
  string name, 
  string description, 
  double value, 
  double minimum, 
  double maximum)
```
| Parameter | Summary |
| --------- | ------- |
| `string` name | The name of the option. |
| `string` description | The description of the option. |
| `double` value | The value of the option. |
| `double` minimum | The minimum the value needs to be |
| `double` maximum | The maximum the value can be. |



## Methods

### Copy
Creates a new object that is a copy of the current instance with the new value\.
```cs
public IOption Copy(
  object value)
```
| Parameter | Summary |
| --------- | ------- |
| `object` value | The new value of the copy. |



## Properties

### Name
The name of the option\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Description
The description of the option\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Minimum
The minimum the value needs to be\.
- **Type:** [System.Double](https://learn.microsoft.com/dotnet/api/system.double)
- **Is Read Only:** `True`

### Maximum
The maximum the value can be\.
- **Type:** [System.Double](https://learn.microsoft.com/dotnet/api/system.double)
- **Is Read Only:** `True`

### Value
The value of the option\.
- **Type:** [System.Double](https://learn.microsoft.com/dotnet/api/system.double)
- **Is Read Only:** `False`
