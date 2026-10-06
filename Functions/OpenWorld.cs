using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Utility;
using UselessPlayerInfo;

namespace UselessPlayerInfo.Functions;

internal static class OpenWorld
{
    private const sbyte ApartmentMainDivisionPlot = -128;
    private const sbyte ApartmentSubdivisionPlot = -127;

    // True while inside a house, apartment, or private room, where map coordinates are meaningless.
    public static unsafe bool IsInsideHousing()
    {
        var housingManager = HousingManager.Instance();
        return housingManager != null && housingManager->IsInside();
    }

    // Check for if on a housing plot or in a building/room and return true if not. 
    public static bool CanSaveCurrentLocation()
    {
        return IsInsideHousing() == false;
    }

    public static string GetCoordsAndLocation()
    {

        var localPlayer = Plugin.ObjectTable.LocalPlayer;
        if (localPlayer != null)
        {
            if (!IsInsideHousing())
            {
                var mapCoords = localPlayer.GetMapCoordinates(false);
                return $"Map: {mapCoords.X:0.0}, {mapCoords.Y:0.0}";
            }
        } 
        else
        {
            return "Not a valid territory";
        }
        return "An Error Has Ocurred";

    }

}
