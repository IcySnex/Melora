# BoolOption
Represents a custom bool option for a plugin config\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging), [IOption](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IOption.html)
```cs
public class BoolOption : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, IOption
```


## Constructors
Creates a new BoolOption\.
```cs
public BoolOption(
  string name, 
  string description, 
  bool value)
```
| Parameter | Summary |
| --------- | ------- |
| `string` name | The name of the option. |
| `string` description | The description of the option. |
| `bool` value | The value of the option. |



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

### Value
The value of the option\.
- **Type:** [System.Boolean](https://learn.microsoft.com/dotnet/api/system.bool)
- **Is Read Only:** `False`
