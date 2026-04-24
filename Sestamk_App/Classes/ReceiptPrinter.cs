using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace Sestamk.Classes
{
    /// <summary>
    /// طباعة الإيصال الحراري — يدعم طابعات ESC/POS + طباعة Windows العادية
    /// </summary>
    public class ReceiptPrinter
    {
        private readonly string _printerName;
        private readonly int _paperWidth; // عرض الورقة بالأحرف (عادة 42 لـ 80mm أو 32 لـ 58mm)

        public ReceiptPrinter(string printerName = "", int paperWidth = 42)
        {
            _printerName = string.IsNullOrEmpty(printerName)
                ? SettingsService.DefaultPrinterName
                : printerName;
            _paperWidth = paperWidth;
        }

        // ═══════════════════════════════════════════════════════
        //  طباعة الإيصال بطريقة Windows PrintDocument (الأسهل)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// طباعة إيصال كامل باستخدام GDI+ (يعمل مع أي طابعة Windows)
        /// </summary>
        public void PrintReceipt(OrderModel order)
        {
            try
            {
                PrintDocument doc = new PrintDocument();

                if (!string.IsNullOrEmpty(_printerName))
                    doc.PrinterSettings.PrinterName = _printerName;

                doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 314, 1200); // 80mm width
                doc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                doc.PrintPage += (sender, e) =>
                {
                    DrawReceipt(e.Graphics, order, e.MarginBounds);
                    e.HasMorePages = false;
                };

                doc.Print();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ طباعة", "خطأ في الطباعة: " + ex.Message);
            }
        }

        /// <summary>
        /// رسم محتوى الإيصال
        /// </summary>
        private void DrawReceipt(Graphics g, OrderModel order, Rectangle bounds)
        {
            // ── الخطوط ──
            var fontTitle = new Font("Alexandria", 14, FontStyle.Bold);
            var fontSubtitle = new Font("Alexandria", 9, FontStyle.Regular);
            var fontNormal = new Font("Alexandria", 9, FontStyle.Regular);
            var fontBold = new Font("Alexandria", 9, FontStyle.Bold);
            var fontLarge = new Font("Alexandria", 12, FontStyle.Bold);

            // ── إعدادات الرسم ──
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

            float y = bounds.Top;
            float lineHeight = 20;
            float width = bounds.Width;
            Brush black = Brushes.Black;
            Pen dashedPen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };

            // ═══ 1. رأس الإيصال ═══
            g.DrawString(SettingsService.StoreName, fontTitle, black,
                new RectangleF(bounds.Left, y, width, 30), sf);
            y += 32;

            if (!string.IsNullOrEmpty(SettingsService.StoreAddress))
            {
                g.DrawString(SettingsService.StoreAddress, fontSubtitle, black,
                    new RectangleF(bounds.Left, y, width, 18), sf);
                y += 20;
            }

            if (!string.IsNullOrEmpty(SettingsService.StorePhone))
            {
                g.DrawString($"تليفون: {SettingsService.StorePhone}", fontSubtitle, black,
                    new RectangleF(bounds.Left, y, width, 18), sf);
                y += 20;
            }

            // خط فاصل
            y += 5;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // ═══ 2. بيانات الفاتورة ═══
            DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                order.OrderDate.ToString("yyyy/MM/dd hh:mm tt"), $"فاتورة: {order.OrderNumber}");

            DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                order.OrderTypeText, $"الكاشير: {order.CashierName}");

            if (order.CustomerName != "عميل نقدي")
            {
                DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                    "", $"العميل: {order.CustomerName}");
            }

            // خط فاصل
            y += 3;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // ═══ 3. رأس الجدول ═══
            DrawRow(g, fontBold, black, bounds.Left, width, ref y,
                "الإجمالي", "الكمية", "السعر", "الصنف");

            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 5;

            // ═══ 4. أصناف الفاتورة ═══
            string currency = SettingsService.CurrencySymbol;
            foreach (var item in order.Items)
            {
                if (item.IsVoided) continue;

                DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{item.LineTotal:N2}",
                    item.Quantity.ToString(),
                    $"{item.UnitPrice:N2}",
                    item.DisplayName);

                // الإضافات
                foreach (var addon in item.Addons)
                {
                    DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                        $"{addon.LineTotal:N2}",
                        addon.Quantity.ToString(),
                        $"{addon.UnitPrice:N2}",
                        $"  + {addon.AddonName}");
                }

                // ملاحظات الصنف
                if (!string.IsNullOrWhiteSpace(item.Notes))
                {
                    g.DrawString($"    📝 {item.Notes}", fontSubtitle, black,
                        new RectangleF(bounds.Left, y, width, 16), sfRight);
                    y += 18;
                }
            }

            // خط فاصل
            y += 3;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // ═══ 5. المجاميع ═══
            DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                $"{order.SubTotal:N2} {currency}", "المجموع الفرعي");

            if (order.ServiceAmount > 0)
            {
                string serviceLabel = order.OrderType == 1 ? "خدمة صالة" : "خدمة توصيل";
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{order.ServiceAmount:N2} {currency}", serviceLabel);
            }

            if (order.DiscountAmount > 0)
            {
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"-{order.DiscountAmount:N2} {currency}", "الخصم");
            }

            if (order.TaxAmount > 0)
            {
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{order.TaxAmount:N2} {currency}", $"ضريبة ({order.TaxPercent}%)");
            }

            // خط فاصل
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // الإجمالي النهائي (كبير)
            DrawTotalRow(g, fontLarge, black, bounds.Left, width, ref y,
                $"{order.TotalAmount:N2} {currency}", "الإجمالي");
            y += 5;

            // المدفوع والباقي
            foreach (var payment in order.Payments)
            {
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{payment.Amount:N2} {currency}", $"المدفوع ({payment.PaymentMethodText})");
            }

            if (order.ChangeAmount > 0)
            {
                DrawTotalRow(g, fontBold, black, bounds.Left, width, ref y,
                    $"{order.ChangeAmount:N2} {currency}", "الباقي");
            }

            if (order.RemainingAmount > 0)
            {
                DrawTotalRow(g, fontBold, black, bounds.Left, width, ref y,
                    $"{order.RemainingAmount:N2} {currency}", "المتبقي (آجل)");
            }

            // ═══ 6. ذيل الإيصال ═══
            y += 10;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 10;

            g.DrawString("شكراً لزيارتكم", fontBold, black,
                new RectangleF(bounds.Left, y, width, 22), sf);
            y += 24;

            g.DrawString("نتمنى لكم تجربة سعيدة", fontSubtitle, black,
                new RectangleF(bounds.Left, y, width, 18), sf);

            // تنظيف الخطوط
            fontTitle.Dispose();
            fontSubtitle.Dispose();
            fontNormal.Dispose();
            fontBold.Dispose();
            fontLarge.Dispose();
            dashedPen.Dispose();
        }

        // ═══ مساعدات الرسم ═══

        private void DrawRow(Graphics g, Font font, Brush brush, float left, float width, ref float y,
            string col1, string col2, string col3 = null, string col4 = null)
        {
            float h = 18;

            if (col3 != null && col4 != null)
            {
                // 4 أعمدة: الصنف | السعر | الكمية | الإجمالي
                float w4 = width * 0.35f;
                float w3 = width * 0.18f;
                float w2 = width * 0.15f;
                float w1 = width * 0.32f;

                var sfR = new StringFormat { Alignment = StringAlignment.Far };
                var sfC = new StringFormat { Alignment = StringAlignment.Center };

                g.DrawString(col4, font, brush, new RectangleF(left + w1 + w2 + w3, y, w4, h), sfR);
                g.DrawString(col3, font, brush, new RectangleF(left + w1 + w2, y, w3, h), sfC);
                g.DrawString(col2, font, brush, new RectangleF(left + w1, y, w2, h), sfC);
                g.DrawString(col1, font, brush, new RectangleF(left, y, w1, h), new StringFormat { Alignment = StringAlignment.Near });
            }
            else
            {
                // 2 أعمدة: يسار | يمين
                float half = width / 2;
                g.DrawString(col1, font, brush, new RectangleF(left, y, half, h), new StringFormat { Alignment = StringAlignment.Near });
                g.DrawString(col2, font, brush, new RectangleF(left + half, y, half, h), new StringFormat { Alignment = StringAlignment.Far });
            }

            y += h + 2;
        }

        private void DrawTotalRow(Graphics g, Font font, Brush brush, float left, float width, ref float y,
            string value, string label)
        {
            float h = 20;
            float half = width / 2;
            g.DrawString(value, font, brush, new RectangleF(left, y, half, h), new StringFormat { Alignment = StringAlignment.Near });
            g.DrawString(label, font, brush, new RectangleF(left + half, y, half, h), new StringFormat { Alignment = StringAlignment.Far });
            y += h + 2;
        }

        // ═══════════════════════════════════════════════════════
        //  فتح درج النقد
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// فتح درج النقد عبر أمر ESC/POS (يُرسل عبر الطابعة الحرارية)
        /// </summary>
        public static void OpenCashDrawer(string printerName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(printerName))
                    printerName = SettingsService.DefaultPrinterName;

                if (string.IsNullOrEmpty(printerName))
                    return;

                // أمر ESC/POS لفتح الدرج (Pin 2)
                byte[] openDrawerCommand = { 0x1B, 0x70, 0x00, 0x19, 0xFA };

                // إرسال الأمر مباشرة للطابعة
                RawPrinterHelper.SendBytesToPrinter(printerName, openDrawerCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening cash drawer: {ex.Message}");
            }
        }

        /// <summary>
        /// الحصول على أسماء الطابعات المتاحة
        /// </summary>
        public static List<string> GetAvailablePrinters()
        {
            var printers = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers;
        }
    }

    /// <summary>
    /// مساعد إرسال بيانات خام للطابعة (للأوامر ESC/POS)
    /// </summary>
    public static class RawPrinterHelper
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pDocName;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pOutputFile;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pDataType;
        }

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [System.Runtime.InteropServices.In] ref DOCINFOA pDocInfo);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr hPrinter;
            DOCINFOA di = new DOCINFOA
            {
                pDocName = "ESC/POS Receipt",
                pDataType = "RAW"
            };

            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                return false;

            try
            {
                if (!StartDocPrinter(hPrinter, 1, ref di))
                    return false;

                if (!StartPagePrinter(hPrinter))
                    return false;

                IntPtr pBytes = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(bytes.Length);
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                bool result = WritePrinter(hPrinter, pBytes, bytes.Length, out int written);

                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pBytes);

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return result;
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }
    }
}
