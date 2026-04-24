using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// خدمة الإعدادات — تجلب القيم من جدول SystemSettings بدل الـ Hardcoded
    /// مع Cache محلي لتجنب الاستعلام المتكرر
    /// </summary>
    public static class SettingsService
    {
        // ═══ Cache محلي — يُحمَّل مرة واحدة عند بدء التشغيل ═══
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static bool _isLoaded = false;

        /// <summary>
        /// تحميل كل الإعدادات في الذاكرة (يُستدعى مرة عند فتح التطبيق)
        /// </summary>
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                // استخدم القيم الافتراضية
                LoadDefaults();
            }
        }

        /// <summary>
        /// تحميل القيم الافتراضية في حالة فشل الاتصال
        /// </summary>
        private static void LoadDefaults()
        {
            _cache["TaxPercent"] = "14";
            _cache["ServicePriceTable"] = "150";
            _cache["ServicePriceDelivery"] = "50";
            _cache["CurrencySymbol"] = "ج.م";
            _cache["CurrencyName"] = "جنية";
            _cache["StoreName"] = "سيستمك";
            _cache["StorePhone"] = "";
            _cache["StoreAddress"] = "";
            _cache["DefaultPrinterName"] = "";
            _cache["PrintReceiptOnPayment"] = "true";
            _cache["OpenDrawerOnPayment"] = "true";
            _cache["RequireShiftToSell"] = "false";
            _isLoaded = true;
        }

        // ═══════════════════════════════════════════════════════
        //  جلب القيم — مع دعم أنواع مختلفة
        // ═══════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════
        //  تحديث إعداد واحد
        // ═══════════════════════════════════════════════════════

        public static async Task SetAsync(string key, string value)
        {
            try
            {
                // MERGE = INSERT or UPDATE
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

                // تحديث الـ Cache
                _cache[key] = value ?? "";
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حفظ الإعداد: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  اختصارات للإعدادات الشائعة
        // ═══════════════════════════════════════════════════════

        public static decimal TaxPercent => GetDecimal("TaxPercent", 14m);
        public static decimal ServicePriceTable => GetDecimal("ServicePriceTable", 20.50m);
        public static decimal ServicePriceDelivery => GetDecimal("ServicePriceDelivery", 50m);
        public static string CurrencySymbol => GetString("CurrencySymbol", "ج.م");
        public static string CurrencyName => GetString("CurrencyName", "جنية");
        public static string StoreName => GetString("StoreName", "سيستمك");
        public static string StorePhone => GetString("StorePhone", "");
        public static string StoreAddress => GetString("StoreAddress", "");
        public static string DefaultPrinterName => GetString("DefaultPrinterName", "");
        public static bool PrintReceiptOnPayment => GetBool("PrintReceiptOnPayment", true);
        public static bool OpenDrawerOnPayment => GetBool("OpenDrawerOnPayment", true);
        public static bool RequireShiftToSell => GetBool("RequireShiftToSell", false);
    }
}
