# IOption
Represents a custom option for a plugin config\.
- **Type:** Interface
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
- **Implements:**  [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged), [INotifyPropertyChanging](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanging)
```cs
public interface IOption : INotifyPropertyChanged, INotifyPropertyChanging
```


## Methods

### Copy
Creates a new object that is a copy of the current instance with the new value\.
```cs
public abstract IOption Copy(
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
- **Type:** [System.Object](https://learn.microsoft.com/dotnet/api/system.object)
- **Is Read Only:** `False`
