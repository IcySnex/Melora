# PluginConfigExtensions
Contains extension methods for plugin configs\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Abstract](/Melora/plugin-api-reference/Melora.Plugins/Abstract/)
```cs
public class PluginConfigExtensions
```




## Methods

### GetOption
Gets the value of the option with the given name\.
```cs
public static T GetOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### GetStringOption
Gets the value of the string option with the given name\.
```cs
public static string GetStringOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### GetIntOption
Gets the value of the int option with the given name\.
```cs
public static int GetIntOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### GetDoubleOption
Gets the value of the double option with the given name\.
```cs
public static double GetDoubleOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### GetBoolOption
Gets the value of the bool option with the given name\.
```cs
public static bool GetBoolOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### GetSelectableOption
Gets the selected value of the selectable option with the given name\.
```cs
public static string GetSelectableOption(
  IPluginConfig config, 
  string name)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config to get the option from. |
| `string` name | The name of the requested option. |

### ContainsAll
Checks if the config contains all given options\.
```cs
public static bool ContainsAll(
  IPluginConfig config, 
  IOption[] options)
```
| Parameter | Summary |
| --------- | ------- |
| `IPluginConfig` config | The config. |
| `IOption[]` options | The options to check for. |



