using FFXIVClientStructs.FFXIV.Client.Game;

namespace UselessPlayerInfo.Functions;

internal static class WeatherHelper
{
    public static unsafe byte GetCurrentWeatherId()
    {
        return WeatherManager.Instance()->GetCurrentWeather();
    }
}
