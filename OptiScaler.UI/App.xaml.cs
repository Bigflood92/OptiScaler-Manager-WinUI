using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using OptiScaler.UI.Views;
using OptiScaler.Core.Services;
using OptiScaler.UI.Dialogs;
using OptiScaler.UI.Services;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;

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

    // Win32 imports for window management
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int SW_RESTORE = 9;

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
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Debug.WriteLine("[App] OnLaunched called");

        try
        {
            // Check for existing instances and redirect if found
            var currentInstance = AppInstance.GetCurrent();
            var activationArgs = currentInstance.GetActivatedEventArgs();
            
            var instances = AppInstance.GetInstances();
            
            // Find any other instance that is not the current one
            var otherInstance = instances.FirstOrDefault(i => !i.IsCurrent);
            if (otherInstance != null)
            {
                Debug.WriteLine("[App] Found existing instance, redirecting and exiting");
                // Redirect to existing instance and terminate this one
                try
                {
                    otherInstance.RedirectActivationToAsync(activationArgs).AsTask().Wait();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[App] Redirect failed: {ex.Message}");
                }
                Environment.Exit(0);
                return;
            }

            // Register activation handler for future activations
            currentInstance.Activated += (_, activationArgs) =>
            {
                Debug.WriteLine("[App] Activated event received (bringing existing window to front)");
                _mainWindow?.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // Bring existing window to foreground
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
                        ShowWindow(hwnd, SW_RESTORE);
                        SetForegroundWindow(hwnd);
                        Debug.WriteLine("[App] Brought window to foreground");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[App] Error bringing window to foreground: {ex.Message}");
                    }
                });
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Single instance check failed: {ex.Message}");
            // Continue with normal startup if single instance check fails
        }

        // This is the first/only instance - create main window
        Debug.WriteLine("[App] This is the primary instance, creating main window");
        _reviewPromptService.RecordAppLaunch();
        
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
        
        Debug.WriteLine("[App] Main window created and activated");
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
