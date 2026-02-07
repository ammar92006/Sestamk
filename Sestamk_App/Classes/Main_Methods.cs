using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace Sestamk.Classes
{
    public class Main_Methods
    {

        public static bool IsInternetAvailable()
        {
			try
			{
				using (Ping myping = new Ping()) 
				{
					PingReply pingReply = myping.Send("8.8.8.8", 2000);
					return (pingReply != null && pingReply.Status == IPStatus.Success);
				}
			}
			catch (Exception)
			{

				return false;
			}
        }
        public static string GetMacAddress()
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var netInterface in networkInterfaces)
                {
                    if (netInterface.OperationalStatus == OperationalStatus.Up &&
                        netInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        netInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        var bytes = netInterface.GetPhysicalAddress().GetAddressBytes();

                        if (bytes.Length == 6)
                        {
                            return BitConverter.ToString(bytes); // AA-BB-CC-DD-EE-FF
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "لا يمكن الحصول على عنوان MAC";
            }

            return "لا يمكن الحصول على عنوان MAC";
        }
        public static string GetExternalIp()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string ip = client.GetStringAsync("https://api.ipify.org").Result;
                    return ip;
                }
            }
            catch
            {
                return "لا يمكن الحصول على IP الخارجي";
            }
        }
        public static string GetMachineGuid()
        {
            try
            {
                using (var regKey = Registry.LocalMachine
                    .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return regKey?.GetValue("MachineGuid")?.ToString() ?? "UNKNOWN_GUID";
                }
            }
            catch
            {
                return "UNKNOWN_GUID";
            }
        }
        public static string GetCpuId()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT ProcessorId FROM Win32_Processor");

                foreach (ManagementObject obj in searcher.Get())
                    return obj["ProcessorId"]?.ToString() ?? "UNKNOWN_CPU";
            }
            catch { }

            return "UNKNOWN_CPU";
        }
        public static string GetMotherboardSerial()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT SerialNumber FROM Win32_BaseBoard");

                foreach (ManagementObject obj in searcher.Get())
                    return obj["SerialNumber"]?.ToString() ?? "UNKNOWN_MB";
            }
            catch { }

            return "UNKNOWN_MB";
        }
        public static string GenerateHWID(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();

                foreach (byte b in bytes)
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }
        }
        public static void OpenForm(Type formType)
        {
            Form openedform = Application.OpenForms
                .Cast<Form>()
                .FirstOrDefault(f => f.GetType() == formType);

            if (openedform != null)
            {
                openedform.WindowState = FormWindowState.Normal;
                openedform.BringToFront();
                openedform.Activate();

            }
            else
            {
                Form frm = (Form)Activator.CreateInstance(formType);
                frm.Show();
            }
        }

    }
}
