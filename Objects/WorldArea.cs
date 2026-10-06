using Dalamud.Configuration;
using System;
using System.Text;

namespace UselessPlayerInfo.Objects;

[Serializable]
public class WorldArea
{
    public string Description { get; set; } = "";
    public int xCord { get; set; }
    public int yCord { get; set; }

    public string ToCode()
    {
        var raw = $"{Description}:{xCord}:{yCord}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryParseCode(string code, out WorldArea? location)
    {
        location = null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim()));
            var parts = raw.Split(':');

            if (parts.Length != 3
                || !int.TryParse(parts[1], out var xcord)
                || !int.TryParse(parts[2], out var ycord))
            {
                return false;
            }

            location = new WorldArea
            {
                Description = parts[0],
                xCord = xcord,
                yCord = ycord,
            };
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}