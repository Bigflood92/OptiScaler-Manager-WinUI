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

    private readonly Windows.Storage.ApplicationDataContainer _localSettings;

    public ReviewPromptService()
    {
        _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    }

    /// <summary>
    /// Call this on app launch to track usage
    /// </summary>
    public void RecordAppLaunch()
    {
        var currentCount = GetLaunchCount();
        _localSettings.Values[LaunchCountKey] = currentCount + 1;
    }

    /// <summary>
    /// Check if we should show the review prompt
    /// </summary>
    public bool ShouldShowReviewPrompt()
    {
        // Don't prompt if user explicitly declined
        if (_localSettings.Values.ContainsKey(UserDeclinedReviewKey) &&
            (bool)_localSettings.Values[UserDeclinedReviewKey])
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
        if (_localSettings.Values.ContainsKey(LastReviewPromptKey))
        {
            var lastPrompt = DateTime.FromBinary((long)_localSettings.Values[LastReviewPromptKey]);
            var daysSince = (DateTime.Now - lastPrompt).TotalDays;
            
            if (daysSince < DaysBetweenPrompts)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Show the review prompt dialog
    /// </summary>
    public async Task<bool> ShowReviewPromptAsync(Microsoft.UI.Xaml.XamlRoot xamlRoot)
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
        _localSettings.Values[LastReviewPromptKey] = DateTime.Now.ToBinary();

        switch (result)
        {
            case ContentDialogResult.Primary:
                await LaunchStoreReviewAsync();
                return true;

            case ContentDialogResult.None: // "Don't Ask Again"
                _localSettings.Values[UserDeclinedReviewKey] = true;
                return false;

            default: // "Maybe Later"
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
        if (_localSettings.Values.ContainsKey(LaunchCountKey))
        {
            return (int)_localSettings.Values[LaunchCountKey];
        }
        return 0;
    }

    /// <summary>
    /// Reset all review prompt tracking (for testing)
    /// </summary>
    public void ResetReviewPromptTracking()
    {
        _localSettings.Values.Remove(LaunchCountKey);
        _localSettings.Values.Remove(LastReviewPromptKey);
        _localSettings.Values.Remove(UserDeclinedReviewKey);
    }
}
