using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;
using Dalamud.Interface;

namespace UselessPlayerInfo.Windows;

public class LocationWindow : Window
{
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

                // Example for quarrying Lumina directly, getting the name of our current area.
                var territoryId = Housing.GetOriginalHouseTerritoryTypeId() ?? Plugin.ClientState.TerritoryType;
                
                //Start Debug Infor
                //ImGui.Text("DEBUG ONLY");
                //ImGui.Separator();
                //ImGui.TextUnformatted($"Debug Territory ID: {territoryId}");
                //ImGui.Spacing();
                //ImGui.TextUnformatted($"Debug is housing instance: {Housing.GetLocationSuffix() != null}");

                if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    if(Housing.GetLocationSuffix() != null)
                    {
                            message = $"You are currently in \"{territoryRow.PlaceName.Value.Name}, {Housing.GetLocationSuffix()}\"";
                    }
                    else
                    {
                        message = $"You are currently in \"{territoryRow.PlaceName.Value.Name}\"";
                    }
                } 
                else
                {
                    message = "Invalid territory.";    
                }


                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                //ImGui.SameLine();
                ImGui.Separator();
                ImGui.Text("Current Location");
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextUnformatted($"{message}");
                ImGui.Spacing();

            }


        }

    }

}
