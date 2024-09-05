# PluginNotLoadedException
Represents errors that occurr during loading plugins\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Exceptions](/Melora/plugin-api-reference/Melora.Plugins/Exceptions/)
- **Implements:**  [Exception](https://learn.microsoft.com/dotnet/api/system.exception), [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)
```cs
public class PluginNotLoadedException : Exception, ISerializable
```


## Constructors
Creates a new instance of PluginNotLoadedException
```cs
public PluginNotLoadedException(
  string pluginPath, 
  Type pluginType, 
  Manifest manifest, 
  Exception innerException)
```
| Parameter | Summary |
| --------- | ------- |
| `string` pluginPath | The path to the plugin archive attempted to load. |
| `Type` pluginType | The type of the plugin attempted to load. |
| `Manifest` manifest | The manifest of the plugin attempted to load if exists. |
| `Exception` innerException | The inner exception. |





## Properties

### PluginPath
The path to the plugin archive attempted to load\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### PluginType
The type of the plugin attempted to load\.
- **Type:** [System.Type](https://learn.microsoft.com/dotnet/api/system.type)
- **Is Read Only:** `True`

### Manifest
The manifest of the plugin attempted to load if exists\.
- **Type:** [Melora.Plugins.Models.Manifest](/Melora/plugin-api-reference/Melora.Plugins/Models/Manifest.html)
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
