# PluginLoadContext
Represents the runtime's concept of a scope for plugin loading\.
- **Type:** Class
- **Namespace:** [Melora.Plugins](/Melora/plugin-api-reference/Melora.Plugins/)
- **Implements:**  [AssemblyLoadContext](https://learn.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext)
```cs
public class PluginLoadContext : AssemblyLoadContext
```




## Methods

### FromPluginArchiveAsync
Creates a new instance of PluginLoadContext from a plugin archive\.
```cs
public static Task<PluginLoadContext> FromPluginArchiveAsync(
  string path, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `string` path | The path to the plugin archive. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |

### LoadFromArchiveAsync
Loads the assembly with an archive\.
```cs
public Task<Assembly> LoadFromArchiveAsync(
  ZipArchive archive, 
  string path, 
  CancellationToken cancellationToken)
```
| Parameter | Summary |
| --------- | ------- |
| `ZipArchive` archive | The archive to load the assembly from. |
| `string` path | The path to the assembly file inside the archive. |
| *(optional)* `CancellationToken` cancellationToken | The token to cancel this action. |



## Properties

### Manifest
The plugin manifest corresponding to this load context\.
- **Type:** [Melora.Plugins.Models.Manifest](/Melora/plugin-api-reference/Melora.Plugins/Models/Manifest.html)
- **Is Read Only:** `True`

### Location
The full path of the file from which the plugin was loaded from\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### EntryPointAssembly
The assembly of the plugin entry point\.
- **Type:** [System.Reflection.Assembly](https://learn.microsoft.com/dotnet/api/system.reflection.assembly)
- **Is Read Only:** `False`

### Assemblies
- **Type:** [System.Collections.Generic.IEnumerable<System.Reflection.Assembly>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable<assembly>)
- **Is Read Only:** `True`

### IsCollectible
- **Type:** [System.Boolean](https://learn.microsoft.com/dotnet/api/system.bool)
- **Is Read Only:** `True`

### Name
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`
