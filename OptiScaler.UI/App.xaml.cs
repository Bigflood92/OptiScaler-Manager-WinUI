using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.ExceptionServices;
using Microsoft.UI.Xaml;
using OptiScaler.UI.Views;
using OptiScaler.Core.Services;
using OptiScaler.UI.Dialogs;
using OptiScaler.UI.Services;
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
    private static Mutex? _instanceMutex;

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
        Debug.WriteLine("[App] Constructor started");
        
        try
        {
            // Register first-chance exception handler
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if (e.Exception is COMException comEx)
                {
                    Debug.WriteLine($"[App] First-chance COM exception: {comEx.Message}");
                    Debug.WriteLine($"[App] HRESULT: 0x{comEx.HResult:X8}");
                    Debug.WriteLine($"[App] Stack trace: {comEx.StackTrace}");
                }
            };

            this.InitializeComponent();
            Debug.WriteLine("[App] InitializeComponent completed");

            _crashReportService = new CrashReportService();
            _reviewPromptService = new ReviewPromptService();

            this.UnhandledException += App_UnhandledException;
            
            Debug.WriteLine("[App] Constructor completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Constructor exception: {ex.GetType().Name}");
            Debug.WriteLine($"[App] Message: {ex.Message}");
            Debug.WriteLine($"[App] Stack: {ex.StackTrace}");
            throw;
        }
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
            // Single instance check using Mutex
            const string mutexName = "OptiScalerManager_SingleInstance_Mutex";
            bool createdNew = false;
            
            try
            {
                _instanceMutex = new Mutex(true, mutexName, out createdNew);
                
                if (!createdNew)
                {
                    Debug.WriteLine("[App] Another instance is already running, exiting");
                    // Another instance is already running
                    Environment.Exit(0);
                    return;
                }
                
                Debug.WriteLine("[App] This is the primary instance (Mutex acquired)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] Mutex check failed: {ex.Message}, continuing anyway");
                // If mutex fails, continue with startup
            }

            // This is the first/only instance - create main window
            Debug.WriteLine("[App] Creating main window");
            _reviewPromptService.RecordAppLaunch();
            
            _mainWindow = new MainWindow();
            Debug.WriteLine("[App] MainWindow instantiated");
            
            _mainWindow.Activate();
            Debug.WriteLine("[App] Main window activated successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] OnLaunched exception: {ex.GetType().Name}");
            Debug.WriteLine($"[App] Message: {ex.Message}");
            Debug.WriteLine($"[App] Stack: {ex.StackTrace}");
            throw;
        }
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        
        Debug.WriteLine($"[App] Unhandled exception: {e.Exception.GetType().Name}");
        Debug.WriteLine($"[App] Message: {e.Exception.Message}");

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
