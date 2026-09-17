using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;
using Dalamud.Interface;

namespace UselessPlayerInfo.Windows;

public class LocationWindow : Window
{
    private const float WeatherIconSize = 24f;

    private static readonly Vector4 SubtleColor = new(0.65f, 0.65f, 0.65f, 1f);

    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public LocationWindow(Plugin plugin)
        : base("Useless Location Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public static void Dispose() { }

    public override void Draw()
    {

        // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // ImRaii takes care of this after the scope ends.
        // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        using (var child = ImRaii.Child("LocInfoWindow", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                ImGui.AlignTextToFramePadding();

                var message = "";
                var housingSuffix = Housing.GetLocationSuffix();

                // Example for quarrying Lumina directly, getting the name of our current area.
                var territoryId = Housing.GetOriginalHouseTerritoryTypeId() ?? Plugin.ClientState.TerritoryType;

                if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    message = housingSuffix != null
                        ? $"You are currently in \"{territoryRow.PlaceName.Value.Name}, {housingSuffix}\""
                        : $"You are currently in \"{territoryRow.PlaceName.Value.Name}\"";
                }
                else
                {
                    message = "Invalid territory.";
                }

                ImGui.TextUnformatted($"{message}");
                ImGui.Spacing();

                ImGui.Separator();
                ImGui.Text("Weather");
                ImGui.Separator();
                ImGui.Spacing();
                DrawWeather();
                ImGui.Spacing();

                ImGui.Separator();
                ImGui.Text("Coordinates");
                ImGui.Separator();
                ImGui.Spacing();
                DrawCoordinates(housingSuffix != null);
                ImGui.Spacing();
            }


        }

    }

    private static void DrawWeather()
    {
        var weatherId = WeatherHelper.GetCurrentWeatherId();

        if (!Plugin.DataManager.GetExcelSheet<Weather>().TryGetRow(weatherId, out var weatherRow))
        {
            ImGui.TextColored(SubtleColor, "Unknown weather.");
            return;
        }

        var icon = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup((uint)weatherRow.Icon)).GetWrapOrEmpty();
        ImGui.Image(icon.Handle, new Vector2(WeatherIconSize, WeatherIconSize));
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(weatherRow.Name.ToString());
    }

    private static void DrawCoordinates(bool isHousing)
    {
        if (isHousing)
        {
            var coords = Housing.GetCurrentHousingCoords();
            if (coords == null)
            {
                ImGui.TextColored(SubtleColor, "Coordinates unavailable.");
                return;
            }

            ImGui.TextUnformatted(Housing.DescribeCoords(coords.Value.Ward, coords.Value.Plot, coords.Value.Room) ?? "Coordinates unavailable.");
            return;
        }

        var localPlayer = Plugin.ObjectTable.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.TextColored(SubtleColor, "Coordinates unavailable.");
            return;
        }

        var mapCoords = localPlayer.GetMapCoordinates(false);
        ImGui.TextUnformatted($"{mapCoords.X:0.0}, {mapCoords.Y:0.0}");
    }

}
