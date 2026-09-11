using Dalamud.Configuration;
using System;

namespace UselessPlayerInfo.Objects;

[Serializable]
public class SavedLocation
{
    public string Name { get; set; } = "";
    public uint TerritoryId { get; set; }
    public uint WorldId { get; set; }
    public int Ward { get; set; }
    public int Plot { get; set; }
    public int Room { get; set; }
}