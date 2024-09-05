# PluginOptionException
Represents errors that occurr when an requested option of a plugin config is invalid\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Exceptions](/Melora/plugin-api-reference/Melora.Plugins/Exceptions/)
- **Implements:**  [Exception](https://learn.microsoft.com/dotnet/api/system.exception), [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)
```cs
public class PluginOptionException : Exception, ISerializable
```


## Constructors
Creates a new instance of PluginOptionException
```cs
public PluginOptionException(
  string requestedOption, 
  Exception innerException)
```
| Parameter | Summary |
| --------- | ------- |
| `string` requestedOption | The name of the rquested config option. |
| `Exception` innerException | The inner exception. |





## Properties

### RequestedOption
The name of the rquested config option\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### TargetSite
- **Type:** [System.Reflection.MethodBase](https://learn.microsoft.com/dotnet/api/system.reflection.methodbase)
- **Is Read Only:** `True`

### Message
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Data
- **Type:** [System.Collections.IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.idictionary)
- **Is Read Only:** `True`

### InnerException
- **Type:** [System.Exception](https://learn.microsoft.com/dotnet/api/system.exception)
- **Is Read Only:** `True`

### HelpLink
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### Source
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### HResult
- **Type:** [System.Int32](https://learn.microsoft.com/dotnet/api/system.int)
- **Is Read Only:** `False`

### StackTrace
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`
