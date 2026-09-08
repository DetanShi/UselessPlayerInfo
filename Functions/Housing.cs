using FFXIVClientStructs.FFXIV.Client.Game;

namespace UselessPlayerInfo.Functions;

internal static class Housing
{
    // Apartment plots are reported with these sentinel values instead of a real plot index.
    private const sbyte ApartmentMainDivisionPlot = -128;
    private const sbyte ApartmentSubdivisionPlot = -127;

    // Returns a ", Ward X, Plot Y" or ", Ward X, Room Y" suffix when the player is standing in
    // a housing ward or inside a house/apartment, otherwise null.
    public static unsafe string? GetLocationSuffix()
    {
        var housingManager = HousingManager.Instance();
        if (housingManager == null || !(housingManager->IsInside() || housingManager->IsOutside()))
        {
            return null;
        }

        var ward = housingManager->GetCurrentWard() + 1;
        var division = housingManager->GetCurrentDivision();
        var plot = housingManager->GetCurrentPlot();
        var room = housingManager->GetCurrentRoom();

        var suffix = division == 2 ? $"Ward {ward} (Subdivision)" : $"Ward {ward}";

        if (plot is ApartmentMainDivisionPlot or ApartmentSubdivisionPlot)
        {
            if (room > 0)
            {
                suffix += $", Room {room}";
            }
        }
        else if (plot >= 0)
        {
            suffix += $", Plot {plot + 1}";
        }

        return suffix;
    }
}
