using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OptiScaler.Core.Services;

public class SystemInfoService
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DISPLAY_DEVICE
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    public (string vendor, string gpuName) DetectGpuVendor()
    {
        try
        {
            var list = EnumerateDisplayAdapters();
            var lower = list.Select(s => s.ToLowerInvariant()).ToList();
            int idx;
            if ((idx = lower.FindIndex(s => s.Contains("nvidia"))) >= 0) return ("nvidia", list[idx]);
            if ((idx = lower.FindIndex(s => s.Contains("amd") || s.Contains("radeon"))) >= 0) return ("amd", list[idx]);
            if ((idx = lower.FindIndex(s => s.Contains("intel"))) >= 0) return ("intel", list[idx]);
            return ("unknown", list.FirstOrDefault() ?? "Unknown GPU");
        }
        catch { return ("unknown", "Unknown GPU"); }
    }

    private static string[] EnumerateDisplayAdapters()
    {
        var results = new System.Collections.Generic.List<string>();
        uint i = 0;
        while (true)
        {
            var dd = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
            if (!EnumDisplayDevices(null, i, ref dd, 0)) break;
            if (!string.IsNullOrWhiteSpace(dd.DeviceString)) results.Add(dd.DeviceString.Trim());
            i++;
        }
        return results.Distinct().ToArray();
    }

    private string GetFriendlyOsName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            var productName = key?.GetValue("ProductName") as string; // e.g. Windows 11 Pro
            var displayVersion = key?.GetValue("DisplayVersion") as string; // e.g. 23H2
            if (!string.IsNullOrWhiteSpace(productName))
            {
                if (!string.IsNullOrWhiteSpace(displayVersion))
                    return $"{productName} {displayVersion}";
                return productName;
            }
        }
        catch { }
        // Fallback: still returns NT 10.0.x for both Win10/11
        return Environment.OSVersion.VersionString;
    }

    public string BuildSystemSummary()
    {
        var sb = new StringBuilder();
        var (vendor, gpuName) = DetectGpuVendor();
        sb.AppendLine($"GPU: {gpuName}");
        // CPU simplificada
        try
        {
            var cpuRaw = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")?.ToLowerInvariant();
            string cpuBrand = cpuRaw switch
            {
                string s when s != null && s.Contains("intel") => "Intel",
                string s when s != null && (s.Contains("amd") || s.Contains("ryzen")) => "AMD",
                _ => "Unknown"
            };
            sb.AppendLine($"CPU: {cpuBrand}");
        }
        catch { sb.AppendLine("CPU: Unknown"); }
        sb.AppendLine($"OS: {GetFriendlyOsName()}");
        return sb.ToString().TrimEnd();
    }
}