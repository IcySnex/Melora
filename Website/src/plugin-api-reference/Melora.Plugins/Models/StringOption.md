# StringOption
Represents a custom string option for a plugin config\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Models](/Melora/plugin-api-reference/Melora.Plugins/Models/)
- **Implements:**  [ObservableObject](https://learn.microsoft.com/dotnet/api/communitytoolkit.mvvm.componentmodel.observableobject), [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging), [IOption](/Melora/plugin-api-reference/Melora.Plugins/Abstract/IOption.html)
```cs
public class StringOption : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, IOption
```


## Constructors
Creates a new StringOption\.
```cs
public StringOption(
  string name, 
  string description, 
  string value, 
  int maxLength, 
  bool isObscured)
```
| Parameter | Summary |
| --------- | ------- |
| `string` name | The name of the option. |
| `string` description | The description of the option. |
| `string` value | The value of the option. |
| `int` maxLength | The max length of the value |
| `bool` isObscured | Whether the value should be obscured in the UI. |



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

### MaxLength
The max length of the value\.
- **Type:** [System.Int32](https://learn.microsoft.com/dotnet/api/system.int)
- **Is Read Only:** `True`

### IsObscured
Whether the value should be obscured in the UI\.
- **Type:** [System.Boolean](https://learn.microsoft.com/dotnet/api/system.bool)
- **Is Read Only:** `True`

### Value
The value of the option\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`
