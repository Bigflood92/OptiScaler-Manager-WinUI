using System;
using Microsoft.UI.Xaml;
using OptiScaler.UI.Views;
using OptiScaler.Core.Services;
using OptiScaler.UI.Dialogs;
using OptiScaler.UI.Services;

#nullable enable

namespace OptiScaler.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _mainWindow;
    private readonly CrashReportService _crashReportService;
    private readonly ReviewPromptService _reviewPromptService;
    
    /// <summary>
    /// Gets the main window instance
    /// </summary>
    public Window? m_window => _mainWindow;

    /// <summary>
    /// Gets the review prompt service
    /// </summary>
    public ReviewPromptService ReviewPromptService => _reviewPromptService;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        
        _crashReportService = new CrashReportService();
        _reviewPromptService = new ReviewPromptService();
        
        this.UnhandledException += App_UnhandledException;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _reviewPromptService.RecordAppLaunch();
        
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        var crashLogPath = await _crashReportService.LogCrashAsync(
            e.Exception,
            $"Thread: {Environment.CurrentManagedThreadId}\nMessage: {e.Message}"
        );

        await _crashReportService.CleanupOldLogsAsync();

        if (_mainWindow != null)
        {
            _mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                CrashDialog.ShowCrashDialog(_mainWindow, e.Exception, crashLogPath);
            });
        }
        else
        {
            Exit();
        }
    }
}
