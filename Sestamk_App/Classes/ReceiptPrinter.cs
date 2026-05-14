using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

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

                int paperWidth = SettingsService.PrinterPaperSize == "58mm" ? 220 : 314;
                doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", paperWidth, 1200); 
                doc.DefaultPageSettings.Margins = new Margins(8, paperWidth > 250 ? 30 : 10, 5, 5); 

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
        /// رسم محتوى الإيصال وإرجاع الارتفاع النهائي
        /// </summary>
        public static float DrawReceipt(Graphics g, OrderModel order, Rectangle bounds)
        {
            // ── الخطوط (أحجام مُحسَّنة للطابعة الحرارية 80mm) ──
            float baseSize = SettingsService.ReceiptFontSize;
            var fontTitle = new Font("Alexandria", baseSize + 4.5f, FontStyle.Bold);
            var fontSubtitle = new Font("Alexandria", baseSize - 0.5f, FontStyle.Regular);
            var fontNormal = new Font("Alexandria", baseSize, FontStyle.Regular);
            var fontBold = new Font("Alexandria", baseSize, FontStyle.Bold);
            var fontLarge = new Font("Alexandria", baseSize + 2.5f, FontStyle.Bold);

            // ── إعدادات الرسم ──
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

            float y = bounds.Top;
            float lineHeight = 20;
            float width = bounds.Width;
            Brush black = Brushes.Black;
            Pen dashedPen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            Pen solidPen = new Pen(Color.Black, 2);
            Pen doublePen = new Pen(Color.Black, 1.5f);

            // ═══ 0. الشعار (اختياري) ═══
            if (SettingsService.ShowLogo && !string.IsNullOrEmpty(SettingsService.ReceiptLogoPath) && File.Exists(SettingsService.ReceiptLogoPath))
            {
                try {
                    Image logo = Image.FromFile(SettingsService.ReceiptLogoPath);
                    float logoH = 60;
                    float logoW = (logo.Width * logoH) / logo.Height;
                    g.DrawImage(logo, bounds.Left + (width - logoW) / 2, y, logoW, logoH);
                    y += logoH + 5;
                } catch { /* Ignore logo errors */ }
            }

            // ═══ 1. رأس الإيصال ═══
            g.DrawLine(solidPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            g.DrawString($"★ {SettingsService.StoreName} ★", fontTitle, black,
                new RectangleF(bounds.Left, y, width, 30), sf);
            y += 32;

            if (!string.IsNullOrEmpty(SettingsService.StoreAddress))
            {
                g.DrawString(SettingsService.StoreAddress, fontSubtitle, black,
                    new RectangleF(bounds.Left, y, width, 18), sf);
                y += 20;
            }

            string phones = SettingsService.StorePhone;
            if (!string.IsNullOrEmpty(SettingsService.StorePhone2))
            {
                if (string.IsNullOrEmpty(phones)) phones = SettingsService.StorePhone2;
                else phones += " - " + SettingsService.StorePhone2;
            }

            if (!string.IsNullOrEmpty(phones))
            {
                g.DrawString($"تليفون: {phones}", fontSubtitle, black,
                    new RectangleF(bounds.Left, y, width, 18), sf);
                y += 20;
            }

            // خط فاصل
            y += 5;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // ═══ 2. بيانات الفاتورة (صف كامل لكل معلومة) ═══
            DrawInfoRow(g, fontNormal, fontBold, black, bounds.Left, width, ref y,
                ":فاتورة", order.OrderNumber);
            DrawInfoRow(g, fontNormal, fontBold, black, bounds.Left, width, ref y,
                ":التاريخ", order.OrderDate.ToString("yyyy/MM/dd  hh:mm tt"));
            DrawInfoRow(g, fontNormal, fontBold, black, bounds.Left, width, ref y,
                ":النوع", order.OrderTypeText);
            
            if (SettingsService.ShowCashier) {
                DrawInfoRow(g, fontNormal, fontBold, black, bounds.Left, width, ref y,
                    ":الكاشير", order.CashierName);
            }

            if (order.CustomerName != "عميل نقدي")
            {
                DrawInfoRow(g, fontNormal, fontBold, black, bounds.Left, width, ref y,
                    ":العميل", order.CustomerName);
            }

            // خط فاصل
            y += 3;
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // ═══ 3+4. أصناف الفاتورة ═══
            string currency = SettingsService.CurrencySymbol;
            if (SettingsService.ReceiptStyle == "Table")
            {
                DrawTableStyleItems(g, order, fontBold, fontNormal, fontSubtitle, black, bounds.Left, width, ref y, currency);
                y += 8;
            }
            else
            {
                // ── رأس الجدول (كلاسيك) ──
                DrawRow(g, fontBold, black, bounds.Left, width, ref y,
                    "الإجمالي", "الكمية", "السعر", "الصنف");

                g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
                y += 5;

                foreach (var item in order.Items)
                {
                    if (item.IsVoided) continue;

                    DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                        $"{item.LineTotal:N2}",
                        item.Quantity.ToString(),
                        $"{item.UnitPrice:N2}",
                        item.DisplayName);

                    foreach (var addon in item.Addons)
                    {
                        DrawRow(g, fontNormal, black, bounds.Left, width, ref y,
                            $"{addon.LineTotal:N2}",
                            addon.Quantity.ToString(),
                            $"{addon.UnitPrice:N2}",
                            $"  + {addon.AddonName}");
                    }

                    if (!string.IsNullOrWhiteSpace(item.Notes))
                    {
                        g.DrawString($"    📝 {item.Notes}", fontSubtitle, black,
                            new RectangleF(bounds.Left, y, width, 16), sfRight);
                        y += 18;
                    }
                }

                y += 3;
                g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
                y += 8;
            }

            // ═══ 5. المجاميع ═══
            DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                $"{order.SubTotal:N2} {currency}", "المجموع الفرعي");

            if (order.ServiceAmount > 0)
            {
                string serviceLabel = order.OrderType == 1 ? "خدمة صالة" : "خدمة توصيل";
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{order.ServiceAmount:N2} {currency}", serviceLabel);
            }

            if (SettingsService.ShowDiscount && order.DiscountAmount > 0)
            {
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"-{order.DiscountAmount:N2} {currency}", "الخصم");
            }

            if (SettingsService.ShowTax && order.TaxAmount > 0)
            {
                DrawTotalRow(g, fontNormal, black, bounds.Left, width, ref y,
                    $"{order.TaxAmount:N2} {currency}", "الضريبة");
            }

            // خط فاصل
            g.DrawLine(dashedPen, bounds.Left, y, bounds.Right, y);
            y += 8;

            // الإجمالي النهائي (كبير داخل إطار)
            y += 5;
            float boxY = y;
            float boxH = 30;
            g.DrawRectangle(doublePen, bounds.Left, boxY, width, boxH);
            
            var sfTotalNear = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            var sfTotalFar = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            
            g.DrawString($"{order.TotalAmount:N2} {currency}", fontLarge, black, new RectangleF(bounds.Left + 5, boxY, (width / 2) - 5, boxH), sfTotalNear);
            g.DrawString("الإجمالي", fontLarge, black, new RectangleF(bounds.Left + (width / 2), boxY, (width / 2) - 5, boxH), sfTotalFar);
            
            y += boxH + 8;

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

            string footerText = SettingsService.ReceiptFooter;
            if (!string.IsNullOrEmpty(footerText)) {
                g.DrawString(footerText, fontSubtitle, black,
                    new RectangleF(bounds.Left, y, width, 60), sf);
                y += 65;
            }

            g.DrawLine(solidPen, bounds.Left, y, bounds.Right, y);
            y += 8;
            g.DrawString("Sestamk POS - sestamk.com", fontSubtitle, black,
                new RectangleF(bounds.Left, y, width, 18), sf);

            // تنظيف الخطوط
            fontTitle.Dispose();
            fontSubtitle.Dispose();
            fontNormal.Dispose();
            fontBold.Dispose();
            fontLarge.Dispose();
            dashedPen.Dispose();
            solidPen.Dispose();
            doublePen.Dispose();

            return y;
        }

        // ═══ رسم الأصناف بشكل جدول (الستايل الثاني) ═══

        private static void DrawTableStyleItems(Graphics g, OrderModel order,
            Font fontBold, Font fontNormal, Font fontSubtitle, Brush black,
            float left, float width, ref float y, string currency)
        {
            float c1W = width * 0.28f;  // الإجمالي (يسار)
            float c2W = width * 0.14f;  // الكمية
            float c3W = width * 0.18f;  // السعر
            float c4W = width * 0.40f;  // الصنف (يمين)
            float rowH = 20f;

            var gridPen   = new Pen(Color.Black, 0.5f);
            var borderPen = new Pen(Color.Black, 1.2f);

            var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var sfR = new StringFormat { Alignment = StringAlignment.Far,    LineAlignment = StringAlignment.Center };
            var sfL = new StringFormat { Alignment = StringAlignment.Near,   LineAlignment = StringAlignment.Center };

            // ── رأس الجدول بخلفية داكنة ──
            using (var headerBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
                g.FillRectangle(headerBrush, left, y, width, rowH);

            DrawTableCellBorders(g, borderPen, left, y, width, rowH, c1W, c2W, c3W);

            g.DrawString("الإجمالي", fontBold, Brushes.White, new RectangleF(left + 2,                     y, c1W - 4, rowH), sfL);
            g.DrawString("الكمية",   fontBold, Brushes.White, new RectangleF(left + c1W,                   y, c2W,     rowH), sfC);
            g.DrawString("السعر",    fontBold, Brushes.White, new RectangleF(left + c1W + c2W,             y, c3W,     rowH), sfC);
            g.DrawString("الصنف",   fontBold, Brushes.White, new RectangleF(left + c1W + c2W + c3W + 2,   y, c4W - 4, rowH), sfR);
            y += rowH;

            // ── صفوف الأصناف ──
            foreach (var item in order.Items)
            {
                if (item.IsVoided) continue;

                DrawTableCellBorders(g, gridPen, left, y, width, rowH, c1W, c2W, c3W);
                g.DrawString($"{item.LineTotal:N2}",  fontNormal, black, new RectangleF(left + 2,                   y, c1W - 4, rowH), sfL);
                g.DrawString(item.Quantity.ToString(), fontNormal, black, new RectangleF(left + c1W,                 y, c2W,     rowH), sfC);
                g.DrawString($"{item.UnitPrice:N2}",  fontNormal, black, new RectangleF(left + c1W + c2W,           y, c3W,     rowH), sfC);
                g.DrawString(item.DisplayName,         fontNormal, black, new RectangleF(left + c1W + c2W + c3W + 2, y, c4W - 4, rowH), sfR);
                y += rowH;

                // الإضافات (صفوف فرعية)
                foreach (var addon in item.Addons)
                {
                    DrawTableCellBorders(g, gridPen, left, y, width, rowH, c1W, c2W, c3W);
                    g.DrawString($"{addon.LineTotal:N2}",  fontNormal, black, new RectangleF(left + 2,                   y, c1W - 4, rowH), sfL);
                    g.DrawString(addon.Quantity.ToString(), fontNormal, black, new RectangleF(left + c1W,                 y, c2W,     rowH), sfC);
                    g.DrawString($"{addon.UnitPrice:N2}",  fontNormal, black, new RectangleF(left + c1W + c2W,           y, c3W,     rowH), sfC);
                    g.DrawString($"+ {addon.AddonName}",   fontNormal, black, new RectangleF(left + c1W + c2W + c3W + 2, y, c4W - 4, rowH), sfR);
                    y += rowH;
                }

                // ملاحظات الصنف
                if (!string.IsNullOrWhiteSpace(item.Notes))
                {
                    g.DrawString($"📝 {item.Notes}", fontSubtitle, black,
                        new RectangleF(left, y, width, 16),
                        new StringFormat { Alignment = StringAlignment.Far });
                    y += 18;
                }
            }

            // خط سفلي للجدول
            g.DrawLine(borderPen, left, y, left + width, y);
            y += 3;

            gridPen.Dispose();
            borderPen.Dispose();
        }

        private static void DrawTableCellBorders(Graphics g, Pen pen,
            float left, float y, float width, float rowH,
            float c1W, float c2W, float c3W)
        {
            g.DrawRectangle(pen, left, y, width, rowH);
            g.DrawLine(pen, left + c1W,             y, left + c1W,             y + rowH);
            g.DrawLine(pen, left + c1W + c2W,       y, left + c1W + c2W,       y + rowH);
            g.DrawLine(pen, left + c1W + c2W + c3W, y, left + c1W + c2W + c3W, y + rowH);
        }

        // ═══ مساعدات الرسم ═══

        private static void DrawRow(Graphics g, Font font, Brush brush, float left, float width, ref float y,
            string col1, string col2, string col3 = null, string col4 = null)
        {
            float h = 18;

            if (col3 != null && col4 != null)
            {
                // 4 أعمدة: الصنف | السعر | الكمية | الإجمالي
                float w4 = width * 0.40f;  // الصنف (أوسع للعربي)
                float w3 = width * 0.18f;  // السعر
                float w2 = width * 0.14f;  // الكمية
                float w1 = width * 0.28f;  // الإجمالي

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

        private static void DrawTotalRow(Graphics g, Font font, Brush brush, float left, float width, ref float y,
            string value, string label)
        {
            float h = 20;
            float half = width / 2;
            g.DrawString(value, font, brush, new RectangleF(left, y, half, h), new StringFormat { Alignment = StringAlignment.Near });
            g.DrawString(label, font, brush, new RectangleF(left + half, y, half, h), new StringFormat { Alignment = StringAlignment.Far });
            y += h + 2;
        }

        /// <summary>
        /// صف معلومات: Label يمين + Value يسار (بعرض كامل — لبيانات الفاتورة)
        /// </summary>
        private static void DrawInfoRow(Graphics g, Font fontValue, Font fontLabel, Brush brush,
            float left, float width, ref float y, string label, string value)
        {
            float h = 18;
            float labelW = width * 0.30f;
            float valueW = width * 0.70f;

            var sfR = new StringFormat { Alignment = StringAlignment.Far };
            var sfL = new StringFormat { Alignment = StringAlignment.Near };

            // Label على اليمين، Value على اليسار
            g.DrawString(label, fontLabel, brush, new RectangleF(left + valueW, y, labelW, h), sfR);
            g.DrawString(value, fontValue, brush, new RectangleF(left, y, valueW, h), sfL);

            y += h;
        }

        // ═══════════════════════════════════════════════════════
        //  توليد الصور والـ PDF
        // ═══════════════════════════════════════════════════════

        public static Bitmap GenerateReceiptImage(OrderModel order)
        {
            int paperWidth = SettingsService.PrinterPaperSize == "58mm" ? 220 : 314;
            int estimatedHeight = 500 + (order.Items.Count * 60) + (order.Payments.Count * 30);
            Bitmap tempBmp = new Bitmap(paperWidth, estimatedHeight);
            float finalY = 0;
            
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                
                Rectangle bounds = new Rectangle(8, 5, paperWidth - 16, estimatedHeight - 10); 
                finalY = DrawReceipt(g, order, bounds);
            }

            int actualHeight = (int)finalY + 20;
            Bitmap finalBmp = new Bitmap(paperWidth, actualHeight);
            using (Graphics g = Graphics.FromImage(finalBmp))
            {
                g.Clear(Color.White);
                g.DrawImage(tempBmp, new Rectangle(0, 0, paperWidth, actualHeight), 
                                     new Rectangle(0, 0, paperWidth, actualHeight), GraphicsUnit.Pixel);
            }
            tempBmp.Dispose();

            return finalBmp;
        }

        public static byte[] GenerateReceiptPdf(OrderModel order)
        {
            using (Bitmap bmp = GenerateReceiptImage(order))
            {
                using (MemoryStream msImg = new MemoryStream())
                {
                    bmp.Save(msImg, System.Drawing.Imaging.ImageFormat.Png);
                    msImg.Position = 0;

                    PdfDocument pdf = new PdfDocument();
                    PdfPage page = pdf.AddPage();
                    
                    page.Width = bmp.Width;
                    page.Height = bmp.Height;

                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    {
                        using (XImage xImage = XImage.FromStream(() => new MemoryStream(msImg.ToArray())))
                        {
                            gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);
                        }
                    }

                    using (MemoryStream msPdf = new MemoryStream())
                    {
                        pdf.Save(msPdf, false);
                        return msPdf.ToArray();
                    }
                }
            }
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
