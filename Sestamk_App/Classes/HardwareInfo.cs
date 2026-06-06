using System;
using System.Management;

namespace Sestamk.Classes
{
    public class HardwareInfo
    {
        public static string GetDeviceName() => Environment.MachineName;

        public static string GetOSInfo()
        {
            // يجيب نسخة الويندوز زي Windows 10 Pro
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject os in searcher.Get())
                    return os["Caption"]?.ToString();
            }
            return Environment.OSVersion.ToString();
        }

        public static string GetProcessorName()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
            {
                foreach (ManagementObject cpu in searcher.Get())
                    return cpu["Name"]?.ToString();
            }
            return "Unknown CPU";
        }

        public static int GetRamGB()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject ram in searcher.Get())
                {
                    long bytes = Convert.ToInt64(ram["TotalPhysicalMemory"]);
                    return (int)(bytes / (1024 * 1024 * 1024)); // تحويل للجيجا
                }
            }
            return 0;
        }
    }
}