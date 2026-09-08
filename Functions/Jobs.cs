using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace UselessPlayerInfo.Functions;

public enum JobRole
{
    Tank,
    Healer,
    Melee,
    PhysicalRanged,
    MagicalRanged,
    Hand,
    Land
}

internal static class Jobs
{
    // Only the final jobs are listed - a job's prerequisite base class (e.g. GLA for PLD) is
    // left out since it's superseded once the job itself is unlocked.
    public static readonly (string Abbrev, uint Id, JobRole Role)[] All = new (string, uint, JobRole)[]
    {
        ("PLD", 19, JobRole.Tank), ("WAR", 21, JobRole.Tank), ("DRK", 32, JobRole.Tank), ("GNB", 37, JobRole.Tank),

        ("WHM", 24, JobRole.Healer), ("SCH", 28, JobRole.Healer), ("AST", 33, JobRole.Healer), ("SGE", 40, JobRole.Healer),

        ("MNK", 20, JobRole.Melee), ("DRG", 22, JobRole.Melee), ("NIN", 30, JobRole.Melee),
        ("SAM", 34, JobRole.Melee), ("RPR", 39, JobRole.Melee), ("VPR", 41, JobRole.Melee),

        ("BRD", 23, JobRole.PhysicalRanged), ("MCH", 31, JobRole.PhysicalRanged), ("DNC", 38, JobRole.PhysicalRanged),

        ("BLM", 25, JobRole.MagicalRanged), ("SMN", 27, JobRole.MagicalRanged),
        ("RDM", 35, JobRole.MagicalRanged), ("BLU", 36, JobRole.MagicalRanged), ("PCT", 42, JobRole.MagicalRanged), // include new jobs if applicable

        ("CRP", 8, JobRole.Hand), ("BSM", 9, JobRole.Hand), ("ARM", 10, JobRole.Hand), ("GSM", 11, JobRole.Hand),
        ("LTW", 12, JobRole.Hand), ("WVR", 13, JobRole.Hand), ("ALC", 14, JobRole.Hand), ("CUL", 15, JobRole.Hand),

        ("MIN", 16, JobRole.Land), ("BTN", 17, JobRole.Land), ("FSH", 18, JobRole.Land)
    };

    // The in-game "job" icon set starts at this base icon ID, offset by the ClassJob row ID.
    private const uint JobIconBaseId = 62100;

    public static uint GetIconId(uint jobId) => JobIconBaseId + jobId;

    // Returns the local player's unlocked jobs and their levels, skipping anything not unlocked or still level 0.
    public static List<(string Abbrev, int Level)> GetUnlockedJobLevels()
    {
        var result = new List<(string Abbrev, int Level)>();

        foreach (var (abbrev, id, _) in All)
        {
            if (!Plugin.DataManager.GetExcelSheet<ClassJob>().TryGetRow(id, out var classJob))
            {
                continue;
            }

            if (!Plugin.UnlockState.IsClassJobUnlocked(classJob))
            {
                continue;
            }

            var level = Plugin.PlayerState.GetClassJobLevel(classJob);
            if (level <= 0)
            {
                continue;
            }

            result.Add((abbrev, level));
        }

        return result;
    }
}
