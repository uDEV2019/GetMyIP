// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace GetMyIP.Helpers;

internal static class MainWindowHelpers
{
    #region Startup
    internal static void GetMyIPStartUp()
    {
        LogStartup();

        EventHandlers();

        MainWindowUIHelpers.ApplyUISettings();

        CommandLineHelpers.ProcessCommandLine();

        SettingsViewModel.ParseInitialPage();
    }
    #endregion Startup

    #region Private fields
    private static readonly Logger _log = LogManager.GetLogger("logTemp");
    #endregion Private fields

    #region MainWindow Instance
    private static readonly MainWindow _mainWindow = Application.Current.MainWindow as MainWindow;
    #endregion MainWindow Instance

    #region StopWatch
    public static Stopwatch _stopwatch = Stopwatch.StartNew();
    #endregion StopWatch

    #region Set and Save MainWindow position and size
    /// <summary>
    /// Sets the MainWindow position and size.
    /// </summary>
    public static void SetWindowPosition()
    {
        Window mainWindow = Application.Current.MainWindow;
        mainWindow.Height = UserSettings.Setting.WindowHeight;
        mainWindow.Left = UserSettings.Setting.WindowLeft;
        mainWindow.Top = UserSettings.Setting.WindowTop;
        mainWindow.Width = UserSettings.Setting.WindowWidth;

        if (UserSettings.Setting.StartCentered)
        {
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    /// <summary>
    /// Saves the MainWindow position and size.
    /// </summary>
    public static void SaveWindowPosition()
    {
        Window mainWindow = Application.Current.MainWindow;
        UserSettings.Setting.WindowHeight = Math.Floor(mainWindow.Height);
        UserSettings.Setting.WindowLeft = Math.Floor(mainWindow.Left);
        UserSettings.Setting.WindowTop = Math.Floor(mainWindow.Top);
        UserSettings.Setting.WindowWidth = Math.Floor(mainWindow.Width);
    }
    #endregion Set and Save MainWindow position and size

    #region Get property value
    /// <summary>
    /// Gets the value of the property
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>An object containing the value of the property</returns>
    public static object GetPropertyValue(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        return prop?.GetValue(sender, null);
    }
    #endregion Get property value

    #region Window Title
    /// <summary>
    /// Puts the version number in the title bar as well as Administrator if running elevated
    /// </summary>
    public static string WindowTitleVersionAdmin()
    {
        // Set the windows title
        if (IsAdministrator())
        {
            return AppInfo.ToolTipVersion + " - (Administrator)";
        }

        return AppInfo.ToolTipVersion;
    }
    #endregion Window Title

    #region Running as Administrator?
    /// <summary>
    /// Determines if running as administrator (elevated)
    /// </summary>
    /// <returns>True if running elevated</returns>
    public static bool IsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
    #endregion Running as Administrator?

    #region Event handlers
    /// <summary>
    /// Event handlers.
    /// </summary>
    internal static void EventHandlers()
    {
        // Unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Settings change events
        UserSettings.Setting.PropertyChanged += SettingChange.UserSettingChanged;
        TempSettings.Setting.PropertyChanged += SettingChange.TempSettingChanged;

        // Window closing event
        _mainWindow.Closing += MainWindow_Closing;
    }
    #endregion Event handlers

    #region Window Events
    private static void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        // Clear any remaining messages
        //_mainWindow.SnackBar1.MessageQueue.Clear();

        // Stop the _stopwatch and record elapsed time
        _stopwatch.Stop();
        _log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {_stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

        // Shut down NLog
        LogManager.Shutdown();

        // Save settings
        SaveWindowPosition();
        ConfigHelpers.SaveSettings();
    }
    #endregion Window Events

    #region Write startup messages to the log
    /// <summary>
    /// Initializes NLog and writes startup messages to the log.
    /// </summary>
    internal static void LogStartup()
    {
        // Set NLog configuration
        NLogHelpers.NLogConfig();

        // Log the version, build date and commit id
        _log.Info($"{AppInfo.AppName} ({AppInfo.AppProduct}) {AppInfo.AppVersion} is starting up");
        _log.Info($"{AppInfo.AppName} {AppInfo.AppCopyright}");
        _log.Debug($"{AppInfo.AppName} Build date: {BuildInfo.BuildDateString} (UTC)");
        _log.Debug($"{AppInfo.AppName} Commit ID: {BuildInfo.CommitIDString}");
        _log.Debug($"{AppInfo.AppName} was started from {AppInfo.AppPath}");
        if (IsAdministrator())
        {
            _log.Debug($"{AppInfo.AppName} is running as Administrator");
        }

        // Log the .NET version and OS platform
        _log.Debug($"Operating System version: {AppInfo.OsPlatform}");
        _log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}");
        _log.Debug("");
    }
    #endregion Write startup messages to the log

    #region Unhandled Exception Handler
    /// <summary>
    /// Handles any exceptions that weren't caught by a try-catch statement.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <remarks>
    /// This uses default message box.
    /// </remarks>
    internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        _log.Error("Unhandled Exception");
        Exception e = (Exception)args.ExceptionObject;
        _log.Error(e.Message);
        if (e.InnerException != null)
        {
            _log.Error(e.InnerException.ToString());
        }
        _log.Error(e.StackTrace);

        _ = MessageBox.Show("An error has occurred. See the log file",
            "ERROR",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
    #endregion Unhandled Exception Handler
}