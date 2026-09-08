using System.Linq;
using System.Net.Http;
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
using UselessPlayerInfo.Functions;
using UselessPlayerInfo.Windows;

namespace UselessPlayerInfo;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IUnlockState UnlockState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;

    private const string MainWindowCMD = "/uselessinfo";
    private const string JobWindowCMD = "/jobinfo";
    private const string WhereAmI = "/whereami";
    private const string LocationsCMD = "/savedlocations";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("UselessInfo");
    private JobWindow JobWindow { get; init; }

    private LocationWindow LocationWindow { get; init; }

    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        JobWindow = new JobWindow(this);
        LocationWindow = new LocationWindow(this);
        MainWindow = new MainWindow(this);  

        WindowSystem.AddWindow(JobWindow);
        WindowSystem.AddWindow(LocationWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(MainWindowCMD, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Very Simple Main Window for this plugin."
        });

        CommandManager.AddHandler(JobWindowCMD, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Locations Window for displaying all the player's Jobs and their corresponding levels."
        });

        CommandManager.AddHandler(WhereAmI, new CommandInfo(OnCommand)
        {
            HelpMessage = "Prints your current location information to chat."
        });

        CommandManager.AddHandler(LocationsCMD, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Locations Window for displaying saved locations. (WIP)"
        });

        // Tell the UI system that we want our windows to be drawn throught he window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainWindowUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"UselessPlayerInfo has been loaded.");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anythign during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainWindowUi;

        WindowSystem.RemoveAllWindows();

        JobWindow.Dispose();

        CommandManager.RemoveHandler(MainWindowCMD);
        CommandManager.RemoveHandler(JobWindowCMD);
        CommandManager.RemoveHandler(LocationsCMD);
        CommandManager.RemoveHandler(WhereAmI);
    }

    private void OnCommand(string command, string args)
    {

        switch (command)
        {
            case MainWindowCMD:
                // In response to the slash command, toggle the display status of our main ui
                MainWindow.Toggle();
                break;
            case JobWindowCMD:
                // In response to the slash command, toggle the display status of our main ui
                JobWindow.Toggle();
                break;
            case WhereAmI:
                PrintWhereAmI();
                break;
            case LocationsCMD:
                LocationWindow.Toggle();
                break;
            default:
                break;
        }

    }

    private void PrintWhereAmI()
    {
        var message = "";

        // Example for quarrying Lumina directly, getting the name of our current area.
        var territoryId = Plugin.ClientState.TerritoryType;
        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
            {
                switch (territoryId)
                {
                    case 1249:
                        message = $"You are currently in \"Private Estate, {Housing.GetLocationSuffix()}\"";
                        break;
                    default:
                        message = $"You are currently in \"{territoryRow.PlaceName.Value.Name}, {Housing.GetLocationSuffix()}\"";
                        break;
                    }

                }
                else
                {
                    message = "Invalid territory.";
                }

        ChatGui.Print(new XivChatEntry
        {
            Message = $"{message}",
            Type = XivChatType.SystemMessage
        });
    }

    public void ToggleMainWindowUi() => MainWindow.Toggle();
    public void ToggleJobWindowUi() => JobWindow.Toggle();
    public void ToggleLocationsUI() => LocationWindow.Toggle();

}
