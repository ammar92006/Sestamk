using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    public static class SettingsService
    {
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static bool _isLoaded = false;

        public static async Task LoadAllAsync()
        {
            try
            {
                string query = "SELECT SettingKey, SettingValue FROM SystemSettings";
                var dt = await DB_Server.GetTableAsync(query);

                _cache.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    string key = row["SettingKey"]?.ToString() ?? "";
                    string value = row["SettingValue"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(key))
                        _cache[key] = value;
                }
                _isLoaded = true;

                DB_Server.Initialize(DB_DataSource, DB_UserID, DB_Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                LoadDefaults();
            }
        }

        private static void LoadDefaults()
        {
            _cache["TaxPercent"] = "14";
            _cache["ServicePriceTable"] = "150";
            _cache["ServicePriceDelivery"] = "50";
            _cache["CurrencySymbol"] = "ج.م";
            _cache["CurrencyName"] = "جنية";
            _cache["StoreName"] = "سيستمك";
            _cache["StorePhone"] = "";
            _cache["StorePhone2"] = "";
            _cache["StoreAddress"] = "";
            _cache["DefaultPrinterName"] = "";
            _cache["PrintReceiptOnPayment"] = "true";
            _cache["OpenDrawerOnPayment"] = "true";
            _cache["RequireShiftToSell"] = "false";
            _cache["Receipt_FontSize"] = "8.5";
            _cache["ShowTax"] = "true";
            _cache["ShowDiscount"] = "true";
            _cache["ShowCashier"] = "true";
            _cache["Receipt_Footer"] = "شكراً لزيارتكم\nنتمنى لكم تجربة سعيدة";
            _cache["Receipt_LogoPath"] = "";
            _cache["ShowLogo"] = "true";
            _cache["Receipt_Style"] = "Classic";
            _cache["DefaultOrderType"] = "Takeaway";
            _cache["DB_DataSource"] = SecureConfig.DbDataSource;
            _cache["DB_UserID"] = SecureConfig.DbUserId;
            _cache["DB_Password"] = SecureConfig.DbPassword;
            _isLoaded = true;
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (!_isLoaded) LoadDefaults();
            return _cache.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public static decimal GetDecimal(string key, decimal defaultValue = 0m)
        {
            string val = GetString(key);
            return decimal.TryParse(val, out decimal result) ? result : defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            string val = GetString(key);
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            string val = GetString(key);
            return bool.TryParse(val, out bool result) ? result : defaultValue;
        }

        public static async Task SetAsync(string key, string value)
        {
            try
            {
                string query = @"
                    IF EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = @Key)
                        UPDATE SystemSettings SET SettingValue = @Value, LastModified = GETDATE() WHERE SettingKey = @Key
                    ELSE
                        INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES (@Key, @Value)";

                await DB_Server.ExecuteAsync(query, new SqlParameter[]
                {
                    new SqlParameter("@Key", key),
                    new SqlParameter("@Value", value ?? "")
                });

                _cache[key] = value ?? "";
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حفظ الإعداد: " + ex.Message);
            }
        }

        public static decimal TaxPercent => GetDecimal("TaxPercent", 14m);
        public static decimal ServicePriceTable => GetDecimal("ServicePriceTable", 20.50m);
        public static decimal ServicePriceDelivery => GetDecimal("ServicePriceDelivery", 50m);
        public static string CurrencySymbol => GetString("CurrencySymbol", "ج.م");
        public static string CurrencyName => GetString("CurrencyName", "جنية");
        public static string StoreName => GetString("StoreName", "سيستمك");
        public static string StorePhone => GetString("StorePhone", "");
        public static string StorePhone2 => GetString("StorePhone2", "");
        public static string StoreAddress => GetString("StoreAddress", "");
        public static string DefaultPrinterName => GetString("DefaultPrinterName", "");
        public static bool PrintReceiptOnPayment => GetBool("PrintReceiptOnPayment", true);
        public static bool OpenDrawerOnPayment => GetBool("OpenDrawerOnPayment", true);
        public static bool RequireShiftToSell => GetBool("RequireShiftToSell", false);

        // WhatsApp Settings
        public static bool   WhatsAppEnabled        => GetBool("WhatsApp_Enabled", false);
        public static string WhatsAppServerUrl       => GetString("WhatsApp_ServerUrl", "http://127.0.0.1:3000");
        public static bool   WhatsAppAutoSendInvoice => GetBool("WhatsApp_AutoSendInvoice", false);
        public static string WhatsAppWelcomeTemplate => GetString("WhatsApp_WelcomeTemplate", "مرحباً بك في {store_name}!");
        public static string WhatsAppInvoiceTemplate => GetString("WhatsApp_InvoiceTemplate", "تم تأكيد طلبك رقم #{order_number}\nالإجمالي: {total} {currency}\nشكراً لزيارتكم 🙏");
        public static string WhatsAppInvoiceFormat   => GetString("WhatsApp_InvoiceFormat", "Text");
        public static int    WhatsAppMaxRetries      => GetInt("WhatsApp_MaxRetries", 5);
        public static int    WhatsAppMaxParallel     => GetInt("WhatsApp_MaxParallel", 3);
        public static int    MaxLoginAttempts        => GetInt("Login_MaxAttempts", 5);

        // Notification & Alert Settings
        public static bool NotificationNewOrderSound   => GetBool("Notification_NewOrderSound", true);
        public static bool NotificationErrorSound      => GetBool("Notification_ErrorSound", true);
        public static bool NotificationPrintFailAlert  => GetBool("Notification_PrintFailAlert", true);
        public static bool NotificationLowStockAlert   => GetBool("Notification_LowStockAlert", true);

        // Printer Settings
        public static string PrinterPaperSize      => GetString("Printer_PaperSize", "80mm");
        public static int    PrinterInvoiceCopies  => GetInt("Printer_InvoiceCopies", 1);
        public static string PrinterCashDrawerPort => GetString("Printer_CashDrawerPort", "");

        // Receipt Customization
        public static float  ReceiptFontSize   => (float)GetDecimal("Receipt_FontSize", 8.5m);
        public static bool   ShowTax           => GetBool("ShowTax", true);
        public static bool   ShowDiscount      => GetBool("ShowDiscount", true);
        public static bool   ShowCashier       => GetBool("ShowCashier", true);
        public static string ReceiptFooter     => GetString("Receipt_Footer", "شكراً لزيارتكم\nنتمنى لكم تجربة سعيدة");
        public static string ReceiptLogoPath   => GetString("Receipt_LogoPath", "");
        public static bool   ShowLogo          => GetBool("ShowLogo", true);
        public static string ReceiptStyle      => GetString("Receipt_Style", "Classic");

        // Sales Settings
        public static bool   SalesEnableDineIn      => GetBool("Sales_EnableDineIn", true);
        public static bool   SalesEnableTakeaway    => GetBool("Sales_EnableTakeaway", true);
        public static bool   SalesEnableDelivery    => GetBool("Sales_EnableDelivery", true);
        public static int    DefaultPilotId         => GetInt("DefaultPilotId", 0);
        public static string DefaultPilotName       => GetString("DefaultPilotName", "");
        public static bool   EnableTax              => GetBool("EnableTax", true);
        public static bool   EnableDiscount         => GetBool("EnableDiscount", true);
        public static decimal DefaultDiscountPercent => GetDecimal("DefaultDiscountPercent", 0m);
        public static bool   PaymentCash            => GetBool("Payment_Cash", true);
        public static bool   PaymentVisa            => GetBool("Payment_Visa", true);
        public static bool   PaymentMaster          => GetBool("Payment_Master", true);
        public static bool   PaymentMada            => GetBool("Payment_Mada", true);

        // Scanner Settings
        public static bool   ScannerEnabled  => GetBool("Scanner_Enabled", false);
        public static string ScannerConnectionType => GetString("Scanner_ConnectionType", "USB");
        public static string ScannerCOMPort  => GetString("Scanner_COMPort", "");
        public static string ScannerBaudRate => GetString("Scanner_BaudRate", "9600");
        public static int    ScannerTimeout  => GetInt("Scanner_Timeout", 100);

        // System Settings
        public static string SystemLanguage         => GetString("System_Language", "العربية");
        public static bool   SystemAutoBackup       => GetBool("System_AutoBackup", false);
        public static string SystemBackupPath       => GetString("System_BackupPath", "");
        public static bool   SystemRunAtStartup     => GetBool("System_RunAtStartup", false);
        public static bool   SystemAutoLogoutEnabled=> GetBool("System_AutoLogoutEnabled", false);
        public static int    SystemAutoLogoutTimer  => GetInt("System_AutoLogoutTimer", 15);

        public static string DefaultOrderType       => GetString("DefaultOrderType", "Takeaway");

        // Database Settings — defaults come from SecureConfig (DPAPI-encrypted local file)
        public static string DB_DataSource => GetString("DB_DataSource", SecureConfig.DbDataSource);
        public static string DB_UserID     => GetString("DB_UserID", SecureConfig.DbUserId);
        public static string DB_Password   => GetString("DB_Password", SecureConfig.DbPassword);

        // Updates Settings
        public static bool AutoUpdateEnabled => GetBool("AutoUpdate_Enabled", true);
    }
}
