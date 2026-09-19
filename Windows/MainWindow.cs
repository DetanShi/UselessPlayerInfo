using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;

namespace UselessPlayerInfo.Windows;

public class MainWindow : Window
{
    private const float IconSize = 24f;
    private const float ButtonHeight = 32f;
    private const float WeatherIconSize = 24f;
    private static readonly Vector4 SubtleColor = new(0.65f, 0.65f, 0.65f, 1f);
    private static readonly Vector4 ErrorColor = new Vector4(0.90f, 0.30f, 0.30f, 1f);
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Useless Main Page", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 315),
            MaximumSize = new Vector2(300, 315)
        };

        this.plugin = plugin;
    }

    public static void Dispose() { }

    public override void Draw()
    {
        using (var child = ImRaii.Child("MainInfoWindow", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                DrawHeader();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                DrawPlayerSummary();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                DrawLocationSummary();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                DrawNavigation();
            }
        }

    }

    private static void DrawHeader()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        ImGui.SetWindowFontScale(1.3f);
        ImGui.TextUnformatted("Useless Player Info");
        ImGui.SetWindowFontScale(1f);

        ImGui.TextColored(SubtleColor, version == null ? "" : $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
    }

    private static void DrawPlayerSummary()
    {
        var localPlayer = Plugin.ObjectTable.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.TextColored(ErrorColor, "Log into a character to see player info.");
            return;
        }

        ImGui.TextUnformatted(localPlayer.Name.TextValue);

        if (localPlayer.ClassJob.IsValid)
        {
            DrawJobIcon(localPlayer.ClassJob.RowId, IconSize);
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted($"{localPlayer.ClassJob.Value.Abbreviation} - Level {localPlayer.Level}");
        }

    }

    private static void DrawLocationSummary()
    {
        var localPlayer = Plugin.ObjectTable.LocalPlayer;
        if (localPlayer != null)
        {
            DrawWeather();
            var territoryId = Housing.GetOriginalHouseTerritoryTypeId() ?? Plugin.ClientState.TerritoryType;
            if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
            {
                var locationSuffix = Housing.GetLocationSuffix();
                var location = locationSuffix != null
                    ? $"{territoryRow.PlaceName.Value.Name}, {locationSuffix}"
                    : territoryRow.PlaceName.Value.Name.ToString();

                ImGui.TextColored(SubtleColor, location);
            }

            if (!Housing.IsInsideHousing())
            {
                var mapCoords = localPlayer.GetMapCoordinates(false);
                ImGui.TextColored(SubtleColor, $"Map: {mapCoords.X:0.0}, {mapCoords.Y:0.0}");
            }
        }

    }

    private void DrawNavigation()
    {
        var width = ImGui.GetContentRegionAvail().X;
        var buttonSize = new Vector2(width, ButtonHeight);

        if (ImGui.Button("Job Levels", buttonSize))
        {
            plugin.ToggleJobWindowUi();
        }

        ImGui.Spacing();

        ImGui.Spacing();

        if (ImGui.Button("Saved Locations", buttonSize))
        {
            plugin.ToggleSavedLocationsUI();
        }
    }

    private static void DrawJobIcon(uint jobId, float size)
    {
        var icon = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(Jobs.GetIconId(jobId))).GetWrapOrEmpty();
        ImGui.Image(icon.Handle, new Vector2(size, size));
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

}
