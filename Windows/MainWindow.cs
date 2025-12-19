using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace UselessPlayerInfo.Windows;

public class MainWindow : Window
{
    private readonly Plugin plugin;

    // Jobs array for reference

    private readonly (string Abbrev, uint Id)[] jobs = new (string, uint)[]
{
        ("GLA", 1), ("PGL", 2), ("MRD", 3), ("LNC", 4), ("ARC", 5), ("CNJ", 6), ("THM", 7),
        ("PLD", 19), ("MNK", 20), ("WAR", 21), ("DRG", 22), ("BRD", 23), ("WHM", 24), ("BLM", 25),
        ("ACN", 26), ("SMN", 27), ("SCH", 28),
        ("ROG", 29), ("NIN", 30), ("MCH", 31), ("DRK", 32), ("AST", 33), ("SAM", 34),
        ("RDM", 35), ("BLU", 36), ("GNB", 37), ("DNC", 38), ("RPR", 39), ("SGE", 40),
        ("VPR", 41), ("PCT", 42) // include new jobs if applicable
};

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Useless Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted($"The random config bool is {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();

        // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // ImRaii takes care of this after the scope ends.
        // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        using (var child = ImRaii.Child("InfoWindow", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {

                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                var localPlayer = Plugin.ClientState.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.TextUnformatted("Our local player is currently not loaded.");
                    return;
                }

                if (!localPlayer.ClassJob.IsValid)
                {
                    ImGui.TextUnformatted("Our current job is currently not valid.");
                    return;
                }

                // If you want to see the Macro representation of this SeString use `ToMacroString()`
                ImGui.TextUnformatted($"{localPlayer.ClassJob.Value.NameEnglish}/{localPlayer.ClassJob.Value.Abbreviation} - and is level {localPlayer.Level}");

                // Example for quarrying Lumina directly, getting the name of our current area.
                var territoryId = Plugin.ClientState.TerritoryType;
                if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    switch (territoryId)
                    {
                        case 1249:
                            ImGui.TextUnformatted($"We are currently in ({territoryId}) \"Private Estate\"");
                            break;
                        default:
                            ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name}\"");
                            break;
                    }
                }
                else
                {
                    ImGui.TextUnformatted("Invalid territory.");
                }

                ImGui.Separator();
                ImGui.TextUnformatted("Job Levels");
                ImGui.Separator();

                if (ImGui.BeginTable("JobLevelTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    // Display job abbreviations and levels in pairs (3 pairs per row)
                    foreach (var (abbrev, id) in jobs)
                    {
                        ImGui.TableNextColumn();
                        ImGui.Text(abbrev);

                        ImGui.TableNextColumn();
                        var level = GetJobLevel(localPlayer, id);
                        ImGui.Text(level > 0 ? level.ToString() : "-");
                    }

                    ImGui.EndTable();
                }

            }


        }

    }
    private int GetJobLevel(Plugin.ClientState.LocalPlayer player, uint jobId)
    {
        // Find the classjob entry that matches this job ID
        var job = player.ClassJobArray?.FirstOrDefault(j => j.Id == jobId);
        return (int)(job?.Level ?? 0);
    }
}
