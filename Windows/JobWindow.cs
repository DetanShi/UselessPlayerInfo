using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Functions;

namespace UselessPlayerInfo.Windows;

public class JobWindow : Window
{
    private const float IconSize = 24f;

    private static readonly (JobRole Role, string Label, Vector4 Color)[] RoleTabs =
    {
        (JobRole.Tank, "TNK", new Vector4(0.35f, 0.60f, 0.95f, 1f)),
        (JobRole.Healer, "HLR", new Vector4(0.40f, 0.80f, 0.45f, 1f)),
        (JobRole.Melee, "M-DPS", new Vector4(0.90f, 0.30f, 0.30f, 1f)),
        (JobRole.PhysicalRanged, "PhysR-DPS", new Vector4(0.85f, 0.65f, 0.20f, 1f)),
        (JobRole.MagicalRanged, "MagR-DPS", new Vector4(0.70f, 0.40f, 0.90f, 1f)),
        (JobRole.Hand, "DoH", new Vector4(0.75f, 0.55f, 0.35f, 1f)),
        (JobRole.Land, "DoL", new Vector4(0.55f, 0.75f, 0.55f, 1f)),
    };

    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public JobWindow(Plugin plugin)
        : base("Useless Jobs Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {

        // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // ImRaii takes care of this after the scope ends.
        // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        using (var child = ImRaii.Child("JobInfoWindow", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {

                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                var localPlayer = Plugin.ObjectTable.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.TextUnformatted("Local player is currently not loaded.");
                    return;
                }

                if (!localPlayer.ClassJob.IsValid)
                {
                    ImGui.TextUnformatted("Your current job is currently not valid.");
                    return;
                }

                DrawJobIcon(localPlayer.ClassJob.RowId, IconSize);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted($"{localPlayer.ClassJob.Value.NameEnglish} ({localPlayer.ClassJob.Value.Abbreviation}) - Level {localPlayer.Level}");

                ImGui.Spacing();
                ImGui.TextUnformatted("Job Levels");
                ImGui.Spacing();

                if (ImGui.BeginTabBar("JobRoleTabs", ImGuiTabBarFlags.None))
                {
                    foreach (var (role, label, color) in RoleTabs)
                    {
                        if (ImGui.BeginTabItem(label, ImGuiTabItemFlags.None))
                        {
                            DrawRoleTab(role, color);
                            ImGui.EndTabItem();
                        }
                    }

                    ImGui.EndTabBar();
                }
            }


        }

    }

    private static void DrawRoleTab(JobRole role, Vector4 color)
    {
        ImGui.Spacing();

        if (ImGui.BeginTable($"JobTable_{role}", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("##Icon", ImGuiTableColumnFlags.WidthFixed, IconSize, 0);
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed, 50f, 0);
            ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, 40f, 0);

            foreach (var (abbrev, id, jobRole) in Jobs.All)
            {
                if (jobRole != role)
                {
                    continue;
                }

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                DrawJobIcon(id, IconSize);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.TextColored(color, abbrev);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                var level = GetJobLevel(id);
                ImGui.TextUnformatted(level > 0 ? level.ToString() : "-");
            }

            ImGui.EndTable();
        }

        ImGui.Spacing();
    }

    private static void DrawJobIcon(uint jobId, float size)
    {
        var icon = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(Jobs.GetIconId(jobId))).GetWrapOrEmpty();
        ImGui.Image(icon.Handle, new Vector2(size, size));
    }

    private static int GetJobLevel(uint jobId)
    {
        if (!Plugin.DataManager.GetExcelSheet<ClassJob>().TryGetRow(jobId, out var classJob))
        {
            return 0;
        }

        return Plugin.PlayerState.GetClassJobLevel(classJob);
    }
}
