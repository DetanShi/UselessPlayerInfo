using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using UselessPlayerInfo.Windows;
using UselessPlayerInfo.Functions;

namespace UselessPlayerInfo.Functions
{

    internal class Helpers
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog Log { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;
        public static void ZoneToast(Plugin plugin)
        {
            var territoryId = Plugin.ClientState.TerritoryType;
            if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
            {
                var zoneName = territoryRow.PlaceName.Value.Name.ToString();

                // Print to chat
                ChatGui.Print(new XivChatEntry
                {
                    Message = $"Hello World! You are in: {zoneName}",
                    Type = XivChatType.SystemMessage
                });
                // Toast notification
                ToastGui.ShowQuest($"Hello World! You are in: {zoneName}");
            }
            else
            {
                ToastGui.ShowQuest($"Hello World! You are not in a valid zone.");
            }
        }
    }
}
