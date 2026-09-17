using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;

namespace UselessPlayerInfo.Windows;

public class MainWindow : Window
{
    private const float IconSize = 24f;
    private const float ButtonHeight = 32f;

    private static readonly Vector4 SubtleColor = new(0.65f, 0.65f, 0.65f, 1f);

    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Useless Info: Main Page", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Size = new Vector2(320, 320);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
    }

    public static void Dispose() { }

    public override void Draw()
    {

        // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // ImRaii takes care of this after the scope ends.
        // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
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
            ImGui.TextColored(SubtleColor, "Log into a character to see player info.");
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

        var territoryId = Housing.GetOriginalHouseTerritoryTypeId() ?? Plugin.ClientState.TerritoryType;
        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        {
            var locationSuffix = Housing.GetLocationSuffix();
            var location = locationSuffix != null
                ? $"{territoryRow.PlaceName.Value.Name}, {locationSuffix}"
                : territoryRow.PlaceName.Value.Name.ToString();

            ImGui.TextColored(SubtleColor, location);
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

        if (ImGui.Button("Current Location", buttonSize))
        {
            plugin.ToggleLocationsUI();
        }

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

}
