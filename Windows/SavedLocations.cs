using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;
using UselessPlayerInfo.Objects;

namespace UselessPlayerInfo.Windows;

public class SavedLocationsWindow : Window
{
    private readonly Plugin plugin;
    private string newName = "";
    private int? pendingRemoveIndex;

    public SavedLocationsWindow(Plugin plugin)
        : base("Useless Saved Locations", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public static void Dispose() { }

    public override void Draw()
    {
        using var child = ImRaii.Child("SavedLocationsWindow", Vector2.Zero, true);
        if (!child.Success)
        {
            return;
        }

        DrawAddRow();
        ImGui.Separator();
        ImGui.Spacing();
        DrawTable();
    }

    private void DrawAddRow()
    {
        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewLocationName", "Name this location...", ref newName, 64);

        ImGui.SameLine();

        using (ImRaii.Disabled(string.IsNullOrWhiteSpace(newName)))
        {
            if (ImGui.Button("Add Current Location"))
            {
                AddCurrentLocation();
            }
        }
    }

    private void DrawTable()
    {
        var locations = plugin.Configuration.SavedLocations;

        if (locations.Count == 0)
        {
            ImGui.TextDisabled("No saved locations yet.");
            return;
        }

        if (!ImGui.BeginTable("SavedLocationsTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp))
        {
            return;
        }

        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0);
        ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthStretch, 0);
        ImGui.TableSetupColumn("##Actions", ImGuiTableColumnFlags.WidthFixed, 60f, 0);
        ImGui.TableHeadersRow();

        for (var i = 0; i < locations.Count; i++)
        {
            var loc = locations[i];

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(loc.Name);

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(DescribeLocation(loc));

            ImGui.TableNextColumn();
            using (ImRaii.PushId(i))
            {
                if (ImGui.SmallButton("Remove"))
                {
                    pendingRemoveIndex = i;
                }
            }
        }

        ImGui.EndTable();

        // Deferred so we don't mutate the list mid-iteration.
        if (pendingRemoveIndex is { } index)
        {
            locations.RemoveAt(index);
            plugin.Configuration.Save();
            pendingRemoveIndex = null;
        }
    }

    private void AddCurrentLocation()
    {
        var territoryId = Housing.GetOriginalHouseTerritoryTypeId() ?? Plugin.ClientState.TerritoryType;
        var coords = Housing.GetCurrentHousingCoords();
        var worldId = Plugin.ObjectTable.LocalPlayer?.CurrentWorld.RowId ?? 0;

        if (coords == null) {
            Plugin.Log.Warning(
                $"Could not determine housing coordinates while saving location '{newName}'. " +
                $"TerritoryId: {territoryId}, WorldId: {worldId}");
        }

        plugin.Configuration.SavedLocations.Add(new SavedLocation
        {
            Name = newName,
            TerritoryId = territoryId,
            WorldId = worldId,
            Ward = coords?.Ward ?? 0,
            Plot = coords?.Plot ?? 0,
            Room = coords?.Room ?? 0,
        });

        Plugin.Log.Debug($"Saving configuration with {plugin.Configuration.SavedLocations.Count.ToString()} saved locations.");

        plugin.Configuration.Save();
        newName = "";
    }

    private static string DescribeLocation(SavedLocation loc)
    {
        if (!Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(loc.TerritoryId, out var territoryRow))
        {
            return "Unknown location";
        }

        var name = territoryRow.PlaceName.Value.Name.ToString();

        if (loc.WorldId != 0 && Plugin.DataManager.GetExcelSheet<World>().TryGetRow(loc.WorldId, out var worldRow))
        {
            name += $" ({worldRow.Name})";
        }

        var coords = Housing.DescribeCoords(loc.Ward, loc.Plot, loc.Room);

        return coords == null ? name : $"{name}, {coords}";
    }
}
