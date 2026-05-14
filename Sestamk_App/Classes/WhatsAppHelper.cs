using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Sestamk.Classes
{
    /// <summary>
    /// مـساعد الواتساب — وظائف تجميل النصوص ومعالجة أرقام الهواتف (V2)
    /// </summary>
    public static class WhatsAppHelper
    {
        /// <summary>
        /// تنظيف رقم الهاتف وإضافة كود الدولة بشكل ديناميكي (لا يعتمد على مصر فقط)
        /// </summary>
        public static string FormatPhoneNumber(string phone, string defaultCountryCode = "")
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

            // Remove all non-digits (keeps just numbers)
            string cleaned = Regex.Replace(phone, @"[^\d]", "");

            // If it starts with 00, assume it's fully international without '+' and remove '00'
            if (cleaned.StartsWith("00")) 
                return cleaned.Substring(2);

            // If we have a default country code, clean it too (e.g., "+20" -> "20")
            string countryCode = Regex.Replace(defaultCountryCode ?? "", @"[^\d]", "");

            // If the user typed a number that starts with 0 (local number),
            // and we have a country code, strip the 0 and prepend the code.
            // E.g., Egypt: 010 -> 2010. KSA: 050 -> 96650
            if (cleaned.StartsWith("0") && !string.IsNullOrEmpty(countryCode))
            {
                return countryCode + cleaned.Substring(1);
            }

            // Otherwise, return as is (could be an international number entered without 00 or +)
            return cleaned;
        }

        /// <summary>
        /// توليد نص فاتورة منسق بشكل احترافي للواتساب
        /// </summary>
        public static string FormatInvoiceMessage(string template, string orderNumber, decimal total, string currency, string storeName, string customer, DateTime date)
        {
            if (string.IsNullOrEmpty(template)) return "";

            return template
                .Replace("{order_number}", orderNumber)
                .Replace("{total}",        total.ToString("N2"))
                .Replace("{currency}",     currency)
                .Replace("{store_name}",   storeName)
                .Replace("{customer}",     customer)
                .Replace("{date}",         date.ToString("yyyy/MM/dd HH:mm"));
        }
    }
}
