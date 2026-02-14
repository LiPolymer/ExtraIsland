using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;

namespace ExtraIsland.Shared;

public static class InjectService {
    public static bool TryGetAddSettingsPageGroupMethod([MaybeNullWhen(false)] out MethodInfo method)
    {
        Type settingsWindowRegistryExtensionsType = typeof(SettingsWindowRegistryExtensions);
        method = settingsWindowRegistryExtensionsType
            .GetMethods()
            .FirstOrDefault(method => (method.ToString()?.Contains("AddSettingsPageGroup") ?? false) && method.GetParameters().Length == 4);
        return method != null;
    }

    public static FieldInfo GetSettingsPageInfoNameField()
    {
        Type settingsPageInfoType = typeof(SettingsPageInfo);
        FieldInfo? field = settingsPageInfoType
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(method => method.ToString()?.Contains("Name") ?? false);
        return field!;
    }

    public static PropertyInfo GetSettingsPageInfoGroupIdProperty()
    {
        Type settingsPageInfoType = typeof(SettingsPageInfo);
        PropertyInfo? property = settingsPageInfoType
            .GetProperties()
            .FirstOrDefault(method => method.ToString()?.Contains("GroupId") ?? false);
        return property!;
    }
}