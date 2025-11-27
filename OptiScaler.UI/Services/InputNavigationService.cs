using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Windows.System;

namespace OptiScaler.UI.Services;

/// <summary>
/// Service for handling gamepad and keyboard navigation throughout the app
/// </summary>
public class InputNavigationService
{
    /// <summary>
    /// Setup complete navigation for a page - includes keyboard shortcuts, gamepad, and click-to-focus
    /// </summary>
    public static void SetupPageNavigation(Page page, Action? onRefresh = null, Action? onSettings = null)
    {
        if (page == null) return;

        SetupKeyboardShortcuts(page, onRefresh, onSettings);
        EnableGamepadNavigation(page);
        EnableClickToFocusForAllControls(page);
    }

    /// <summary>
    /// Setup keyboard shortcuts for a page
    /// </summary>
    public static void SetupKeyboardShortcuts(Page page, Action? onRefresh = null, Action? onSettings = null)
    {
        if (page == null) return;

        page.PreviewKeyDown += (sender, e) =>
        {
            // F5 - Refresh
            if (e.Key == VirtualKey.F5 && onRefresh != null)
            {
                e.Handled = true;
                onRefresh();
            }
            
            // Ctrl+R - Refresh
            if (e.Key == VirtualKey.R && 
                (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0 &&
                onRefresh != null)
            {
                e.Handled = true;
                onRefresh();
            }
            
            // Ctrl+, (Comma) - Settings
            if (e.Key == (VirtualKey)188 && 
                (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0 &&
                onSettings != null)
            {
                e.Handled = true;
                onSettings();
            }
        };
    }

    /// <summary>
    /// Setup gamepad navigation for a container
    /// </summary>
    public static void EnableGamepadNavigation(UIElement element)
    {
        if (element == null) return;

        // Enable XY focus navigation
        element.XYFocusKeyboardNavigation = XYFocusKeyboardNavigationMode.Enabled;
        element.TabFocusNavigation = KeyboardNavigationMode.Cycle;
    }

    /// <summary>
    /// Set focus to first focusable element in container
    /// </summary>
    public static void SetInitialFocus(UIElement container)
    {
        if (container == null) return;

        var firstFocusable = FocusManager.FindFirstFocusableElement(container);
        if (firstFocusable is Control control)
        {
            control.Focus(FocusState.Programmatic);
        }
    }

    /// <summary>
    /// Setup click-to-focus behavior for a single control
    /// </summary>
    public static void EnableClickToFocus(Control control)
    {
        if (control == null) return;

        control.PointerPressed += (sender, e) =>
        {
            if (sender is Control ctrl && ctrl.IsTabStop)
            {
                ctrl.Focus(FocusState.Pointer);
            }
        };
    }

    /// <summary>
    /// Recursively enable click-to-focus for all interactive controls in a container
    /// </summary>
    public static void EnableClickToFocusForAllControls(DependencyObject container)
    {
        if (container == null) return;

        var controlsToEnhance = new List<Control>();
        FindFocusableControls(container, controlsToEnhance);

        foreach (var control in controlsToEnhance)
        {
            EnableClickToFocus(control);
        }
    }

    /// <summary>
    /// Recursively find all focusable controls in a visual tree
    /// </summary>
    private static void FindFocusableControls(DependencyObject parent, List<Control> controls)
    {
        if (parent == null) return;

        var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        
        for (int i = 0; i < childCount; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            
            if (child is Control control && 
                (control is Button || 
                 control is TextBox || 
                 control is ComboBox || 
                 control is ToggleSwitch || 
                 control is Slider || 
                 control is CheckBox || 
                 control is RadioButton ||
                 control is ListViewItem))
            {
                if (control.IsTabStop)
                {
                    controls.Add(control);
                }
            }
            
            FindFocusableControls(child, controls);
        }
    }

    /// <summary>
    /// Setup TabIndex for proper tab navigation order
    /// </summary>
    public static void SetupTabSequence(params Control[] controls)
    {
        for (int i = 0; i < controls.Length; i++)
        {
            if (controls[i] != null)
            {
                controls[i].TabIndex = i + 1;
                controls[i].IsTabStop = true;
            }
        }
    }

    /// <summary>
    /// Enable focus visual for better keyboard/gamepad navigation feedback
    /// </summary>
    public static void EnhanceFocusVisuals(Control control)
    {
        if (control == null) return;

        control.UseSystemFocusVisuals = true;
    }
}
