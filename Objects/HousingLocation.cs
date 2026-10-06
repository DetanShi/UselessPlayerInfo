using Dalamud.Configuration;
using System;
using System.Text;

namespace UselessPlayerInfo.Objects;

[Serializable]
public class HousingLocation
{
    public string Name { get; set; } = "";
    public uint TerritoryId { get; set; }
    public uint WorldId { get; set; }
    public int Ward { get; set; }
    public int Plot { get; set; }
    public int Room { get; set; }

    public string ToCode()
    {
        var raw = $"{TerritoryId}:{WorldId}:{Ward}:{Plot}:{Room}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryParseCode(string code, out HousingLocation? location)
    {
        location = null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim()));
            var parts = raw.Split(':');

            if (parts.Length != 5
                || !uint.TryParse(parts[0], out var territoryId)
                || !uint.TryParse(parts[1], out var worldId)
                || !int.TryParse(parts[2], out var ward)
                || !int.TryParse(parts[3], out var plot)
                || !int.TryParse(parts[4], out var room))
            {
                return false;
            }

            location = new HousingLocation
            {
                TerritoryId = territoryId,
                WorldId = worldId,
                Ward = ward,
                Plot = plot,
                Room = room,
            };
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}