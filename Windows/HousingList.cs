using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;
using UselessPlayerInfo.Objects;

namespace UselessPlayerInfo.Windows;

public class HousingListWindow : Window
{
    private readonly Plugin plugin;
    private string newName = "";
    private string importCode = "";
    private string importName = "";
    private string? importError;
    private int? pendingRemoveIndex;

    private static readonly Vector4 ErrorColor = new Vector4(0.90f, 0.30f, 0.30f, 1f);

    public HousingListWindow(Plugin plugin)
        : base("Useless Housing List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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

        var footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        using (var tableChild = ImRaii.Child("SavedLocationsTableRegion", new Vector2(0, -footerHeight)))
        {
            if (tableChild.Success)
            {
                DrawTable();
            }
        }

        ImGui.Separator();
        DrawImportRow();
    }

    private void DrawAddRow()
    {

        if (!Housing.CanSaveCurrentLocation())
        {
            ImGui.TextColored(ErrorColor,"You must be on a plot or in an apartment/private room to save a location.");
        } 
        else
        {
            ImGui.SetNextItemWidth(200f);
            ImGui.InputTextWithHint("##NewLocationName", "Name this location...", ref newName, 64);

            ImGui.SameLine();

            var canSave = !string.IsNullOrWhiteSpace(newName) && Housing.CanSaveCurrentLocation();

            using (ImRaii.Disabled(!canSave))
            {
                if (ImGui.Button("Add Current Location"))
                {
                    AddCurrentLocation();
                }
            }
        }
    }

    private void DrawImportRow()
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.FileImport.ToIconString()))
            {
                importCode = "";
                importName = "";
                importError = null;
                ImGui.OpenPopup("ImportLocationPopup");
            }
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Import a location from a code");
        }

        if (ImGui.BeginPopup("ImportLocationPopup"))
        {
            ImGui.SetNextItemWidth(220f);
            ImGui.InputTextWithHint("##ImportCode", "Paste a location code...", ref importCode, 256);

            ImGui.SetNextItemWidth(220f);
            ImGui.InputTextWithHint("##ImportName", "Name this location...", ref importName, 64);

            var canImport = !string.IsNullOrWhiteSpace(importCode) && !string.IsNullOrWhiteSpace(importName);

            using (ImRaii.Disabled(!canImport))
            {
                if (ImGui.Button("Import"))
                {
                    ImportLocation();
                    if (importError == null)
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            if (importError != null)
            {
                ImGui.TextColored(new Vector4(0.90f, 0.30f, 0.30f, 1f), importError);
            }

            ImGui.EndPopup();
        }
    }

    private void ImportLocation()
    {
        if (!HousingLocation.TryParseCode(importCode, out var location) || location == null)
        {
            importError = "Invalid Code.";
            return;
        }

        if (!Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(location.TerritoryId, out _))
        {
            importError = "The code currently references an unknown territory.";
            return;
        }

        if (location.WorldId != 0 && !Plugin.DataManager.GetExcelSheet<World>().TryGetRow(location.WorldId, out _))
        {
            importError = "The code currently references an unknown world.";
            return;
        }

        location.Name = importName;
        plugin.Configuration.HousingLocations.Add(location);
        plugin.Configuration.Save();

        importCode = "";
        importName = "";
        importError = null;
    }

    private void DrawTable()
    {
        var locations = plugin.Configuration.HousingLocations;

        if (locations.Count == 0)
        {
            ImGui.TextDisabled("No saved locations yet.");
            return;
        }

        if (!ImGui.BeginTable("SavedLocationsTable", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp))
        {
            return;
        }

        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0);
        ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthStretch, 0);
        ImGui.TableSetupColumn("##Share", ImGuiTableColumnFlags.WidthFixed, 30f, 0);
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
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.SmallButton(FontAwesomeIcon.Copy.ToIconString()))
                    {
                        ImGui.SetClipboardText(loc.ToCode());
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Copy shareable code");
                }
            }

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

        plugin.Configuration.HousingLocations.Add(new HousingLocation
        {
            Name = newName,
            TerritoryId = territoryId,
            WorldId = worldId,
            Ward = coords?.Ward ?? 0,
            Plot = coords?.Plot ?? 0,
            Room = coords?.Room ?? 0,
        });

        Plugin.Log.Debug($"Saving configuration with {plugin.Configuration.HousingLocations.Count.ToString()} saved locations.");

        plugin.Configuration.Save();
        newName = "";
    }

    private static string DescribeLocation(HousingLocation loc)
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
