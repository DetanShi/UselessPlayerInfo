using Dalamud.Configuration;
using System.Collections.Generic;
using System;

namespace UselessPlayerInfo.Objects;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public List<SavedLocation> SavedLocations { get; set; } = new();

    public void Save()
    {
        try
        {
            Plugin.PluginInterface.SavePluginConfig(this);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to save plugin configuration.");
        }

    }
}
