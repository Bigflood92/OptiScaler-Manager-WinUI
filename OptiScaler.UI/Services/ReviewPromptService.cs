using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Services.Store;

namespace OptiScaler.UI.Services;

/// <summary>
/// Service for prompting users to rate and review the app in Microsoft Store
/// </summary>
public class ReviewPromptService
{
    private const string LaunchCountKey = "AppLaunchCount";
    private const string LastReviewPromptKey = "LastReviewPromptDate";
    private const string UserDeclinedReviewKey = "UserDeclinedReview";
    private const int LaunchesBeforePrompt = 5;
    private const int DaysBetweenPrompts = 30;

    private Windows.Storage.ApplicationDataContainer? _localSettings;

    private Windows.Storage.ApplicationDataContainer LocalSettings
    {
        get
        {
            if (_localSettings == null)
            {
                try
                {
                    _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                }
                catch
                {
                    // Fallback if ApplicationData.Current is not available (e.g., during debugging)
                    return null!;
                }
            }
            return _localSettings;
        }
    }

    public ReviewPromptService()
    {
        // Lazy initialization - don't access LocalSettings in constructor
    }

    /// <summary>
    /// Call this on app launch to track usage
    /// </summary>
    public void RecordAppLaunch()
    {
        if (LocalSettings == null) return;

        try
        {
            var currentCount = GetLaunchCount();
            LocalSettings.Values[LaunchCountKey] = currentCount + 1;
        }
        catch
        {
            // Silently fail if settings are not accessible
        }
    }

    /// <summary>
    /// Check if we should show the review prompt
    /// </summary>
    public bool ShouldShowReviewPrompt()
    {
        if (LocalSettings == null) return false;

        try
        {
            // Don't prompt if user explicitly declined
            if (LocalSettings.Values.ContainsKey(UserDeclinedReviewKey) &&
                (bool)LocalSettings.Values[UserDeclinedReviewKey])
            {
                return false;
            }

            // Check launch count
            var launchCount = GetLaunchCount();
            if (launchCount < LaunchesBeforePrompt)
            {
                return false;
            }

            // Check time since last prompt
            if (LocalSettings.Values.ContainsKey(LastReviewPromptKey))
            {
                var lastPrompt = DateTime.FromBinary((long)LocalSettings.Values[LastReviewPromptKey]);
                var daysSince = (DateTime.Now - lastPrompt).TotalDays;
                
                if (daysSince < DaysBetweenPrompts)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Show the review prompt dialog
    /// </summary>
    public async Task<bool> ShowReviewPromptAsync(Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        if (LocalSettings == null) return false;

        try
        {
            var dialog = new ContentDialog
            {
                Title = "Enjoying OptiScaler Manager?",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "We'd love to hear your feedback!",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = "Your review helps us improve and helps other gamers discover OptiScaler Manager.",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                Windows.UI.Color.FromArgb(255, 204, 204, 204)),
                            FontSize = 12,
                            Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0)
                        }
                    }
                },
                PrimaryButtonText = "Rate & Review",
                SecondaryButtonText = "Maybe Later",
                CloseButtonText = "Don't Ask Again",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            // Update last prompt time
            LocalSettings.Values[LastReviewPromptKey] = DateTime.Now.ToBinary();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    await LaunchStoreReviewAsync();
                    return true;

                case ContentDialogResult.None: // "Don't Ask Again"
                    LocalSettings.Values[UserDeclinedReviewKey] = true;
                    return false;

                default: // "Maybe Later"
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Launch Microsoft Store to the review page
    /// </summary>
    public async Task<bool> LaunchStoreReviewAsync()
    {
        try
        {
            var storeContext = StoreContext.GetDefault();
            var result = await storeContext.RequestRateAndReviewAppAsync();
            
            return result.Status == StoreRateAndReviewStatus.Succeeded;
        }
        catch
        {
            // Fallback: Open Store page directly
            try
            {
                var packageFamilyName = Package.Current.Id.FamilyName;
                var storeUri = new Uri($"ms-windows-store://review/?PFN={packageFamilyName}");
                return await Windows.System.Launcher.LaunchUriAsync(storeUri);
            }
            catch
            {
                return false;
            }
        }
    }

    private int GetLaunchCount()
    {
        if (LocalSettings == null) return 0;

        try
        {
            if (LocalSettings.Values.ContainsKey(LaunchCountKey))
            {
                return (int)LocalSettings.Values[LaunchCountKey];
            }
        }
        catch
        {
            // Return 0 if settings are corrupted or inaccessible
        }
        
        return 0;
    }

    /// <summary>
    /// Reset all review prompt tracking (for testing)
    /// </summary>
    public void ResetReviewPromptTracking()
    {
        if (LocalSettings == null) return;

        try
        {
            LocalSettings.Values.Remove(LaunchCountKey);
            LocalSettings.Values.Remove(LastReviewPromptKey);
            LocalSettings.Values.Remove(UserDeclinedReviewKey);
        }
        catch
        {
            // Silently fail if settings are not accessible
        }
    }
}
