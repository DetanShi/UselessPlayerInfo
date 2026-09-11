using System.Numerics;
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
    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Useless Info: Main Page", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 150),
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
        using (var child = ImRaii.Child("MainInfoWindow", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {

                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                //ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Spacing();
                if (ImGui.Button("Show Job Window"))
                {
                    plugin.ToggleJobWindowUi();
                }
                ImGui.Spacing();
                if (ImGui.Button("Show Location Window"))
                {
                    plugin.ToggleLocationsUI();
                }
                ImGui.Spacing();
                if (ImGui.Button("Show Saved Locations Window"))
                {
                    plugin.ToggleSavedLocationsUI();
                }

            }


        }

    }

}
