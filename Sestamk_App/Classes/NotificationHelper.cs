using System;

namespace Sestamk.Classes
{
    /// <summary>
    /// كلاس مركزي للأصوات والتنبيهات — يقرأ إعدادات الإشعارات من SettingsService
    /// قبل تنفيذ أي صوت أو تنبيه. يُستخدم في جميع أنحاء البرنامج بدلاً من
    /// الاستدعاء المباشر لـ SystemSounds أو ToastManager
    /// </summary>
    public static class NotificationHelper
    {
        // ═══════════════════════════════════════════════════════════════════
        //  الأصوات
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// تشغيل صوت طلب جديد / إضافة منتج — يُفحص الإعداد أولاً
        /// </summary>
        public static void PlayNewOrderSound()
        {
            if (SettingsService.NotificationNewOrderSound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        /// <summary>
        /// تشغيل صوت الخطأ / التحذير — يُفحص الإعداد أولاً
        /// </summary>
        public static void PlayErrorSound()
        {
            if (SettingsService.NotificationErrorSound)
                System.Media.SystemSounds.Exclamation.Play();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  التنبيهات (Toast Notifications)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// عرض تنبيه فشل الطباعة — يُفحص الإعداد أولاً
        /// </summary>
        public static void NotifyPrintFailure(string details)
        {
            if (SettingsService.NotificationPrintFailAlert)
            {
                ToastManager.ShowWarning("فشل الطباعة ⚠️", details);
                PlayErrorSound();
            }
        }

        /// <summary>
        /// عرض تنبيه نقص المخزون — يُفحص الإعداد أولاً
        /// </summary>
        public static void NotifyLowStock(string productName, int currentQty = 0)
        {
            if (SettingsService.NotificationLowStockAlert)
            {
                string message = currentQty > 0
                    ? $"المنتج \"{productName}\" — الكمية المتبقية: {currentQty}"
                    : $"المنتج \"{productName}\" على وشك النفاد";

                ToastManager.ShowWarning("نقص المخزون ⚠️", message);
                PlayErrorSound();
            }
        }
    }
}
