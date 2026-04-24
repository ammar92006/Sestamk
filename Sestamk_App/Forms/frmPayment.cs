using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmPayment : BaseForm
    {
        // ═══════════════════════════════════════════════════════
        //  Properties — بيانات الفاتورة من فورم المبيعات
        // ═══════════════════════════════════════════════════════
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal TotalAmount { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal ServiceAmount { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal DiscountPercent { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int InvoiceType { get; set; } // 0=تيك اوي, 1=صالة, 2=دليفري
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Customer SelectedCustomer { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string InvoiceNumber { get; set; }

        // ═══════════════════════════════════════════════════════
        //  Output — نتائج الدفع
        // ═══════════════════════════════════════════════════════
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal PaidAmount { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal ChangeAmount { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PaymentMethod { get; private set; } // 0=نقدي, 1=بطاقة, 2=آجل
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal NetTotal { get; private set; } // 🆕 الإجمالي بعد الخصم
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal DiscountAmount { get; private set; } // 🆕 مبلغ الخصم

        // ═══════════════════════════════════════════════════════
        //  متغيرات داخلية
        // ═══════════════════════════════════════════════════════
        private decimal _netTotal; // الإجمالي بعد الخصم
        private int _selectedPaymentMethod = 0; // نقدي افتراضياً

        public frmPayment()
        {
            InitializeComponent();
            SetupForm();
        }

        // ═══════════════════════════════════════════════════════
        //  إعداد الفورم
        // ═══════════════════════════════════════════════════════
        private void SetupForm()
        {
            // Guna2 Borderless Form
            var borderlessForm = new Guna.UI2.WinForms.Guna2BorderlessForm();
            borderlessForm.ContainerControl = this;
            borderlessForm.BorderRadius = 20;
            borderlessForm.AnimateWindow = true;

            this.DoubleBuffered = true;
            this.Load += FrmPayment_Load;

            // ── ربط أزرار لوحة الأرقام ──
            guna2Button13.Click += NumPad_Click; // 7
            guna2Button9.Click += NumPad_Click;  // 8
            guna2Button2.Click += NumPad_Click;  // 9
            guna2Button12.Click += NumPad_Click; // 4
            guna2Button8.Click += NumPad_Click;  // 5
            guna2Button3.Click += NumPad_Click;  // 6
            guna2Button11.Click += NumPad_Click; // 1
            guna2Button7.Click += NumPad_Click;  // 2
            guna2Button4.Click += NumPad_Click;  // 3
            guna2Button6.Click += NumPad_Click;  // 0
            guna2Button10.Click += NumPad_Click; // .
            guna2Button5.Click += NumPad_Clear;  // C

            // ── أزرار المبالغ السريعة ──
            guna2Button17.Click += QuickAmount_Click; // +10
            guna2Button18.Click += QuickAmount_Click; // +50
            guna2Button19.Click += QuickAmount_Click; // +100
            guna2Button20.Click += QuickAmount_Click; // +200

            // ── زر المبلغ بالضبط ──
            guna2Button14.Click += ExactAmount_Click;

            // ── زر تأكيد الدفع ──
            guna2Button16.Click += ConfirmPayment_Click;

            // ── زر إلغاء ──
            guna2Button15.Click += CancelPayment_Click;

            // ── زر إغلاق (X) ──
            guna2Button1.Click += CancelPayment_Click;

            // ── زر مسح آخر رقم (X الدائري) ──
            guna2CircleButton1.Click += Backspace_Click;

            // ── أزرار طريقة الدفع ──
            btnSaffari.Click += PaymentMethod_Click; // نقدي
            btnTable.Click += PaymentMethod_Click;   // كاش
            btnDelivery.Click += PaymentMethod_Click; // آجل

            // ── حدث تغيير الخصم ──
            guna2TextBox1.TextChanged += Discount_TextChanged;

            // ── حدث تغيير المبلغ المستلم ──
            guna2TextBox2.TextChanged += ReceivedAmount_TextChanged;
        }

        // ═══════════════════════════════════════════════════════
        //  عند تحميل الفورم
        // ═══════════════════════════════════════════════════════
        private void FrmPayment_Load(object sender, EventArgs e)
        {
            // تعيين رقم الفاتورة
            if (!string.IsNullOrWhiteSpace(InvoiceNumber))
                label2.Text = $"فاتورة رقم: {InvoiceNumber}";
            else
                label2.Text = $"فاتورة رقم: INV-{DateTime.Now:yyyyMMdd-HHmmss}";

            // تعيين اسم الكاشير
            label4.Text = $"الكاشير: {UserSession.Full_Name ?? "غير محدد"}";

            // تعيين طريقة الدفع الافتراضية (نقدي)
            UpdatePaymentMethodSelection(btnSaffari);
            _selectedPaymentMethod = 0;

            // التحقق من تفعيل زر الآجل
            UpdateCreditButtonState();

            // تعيين الخصم الافتراضي
            guna2TextBox1.Text = "0";

            // حساب المبالغ
            RecalculateAll();

            // مسح خانة المبلغ المستلم
            guna2TextBox2.Text = "";
        }

        // ═══════════════════════════════════════════════════════
        //  حساب كل المبالغ
        // ═══════════════════════════════════════════════════════
        private void RecalculateAll()
        {
            decimal discount = 0;
            if (decimal.TryParse(guna2TextBox1.Text, out decimal d))
                discount = d;

            _netTotal = TotalAmount + ServiceAmount - discount;
            if (_netTotal < 0) _netTotal = 0;

            // 🆕 تحديث الـ Properties
            NetTotal = _netTotal;
            DiscountAmount = discount;

            // المجموع الكلي
            string currency = SettingsService.CurrencySymbol;
            label5.Text = $"{TotalAmount + ServiceAmount:N2} {currency}";
            label3.Text = "المجموع الكلي";

            // الخصم (في guna2TextBox1)
            label9.Text = "الخصم :";

            // الإجمالي بعد الخصم
            label15.Text = $"{_netTotal:N2}";
            label7.Text = "الاجمالي بعد الخصم :";

            // المدفوع
            decimal received = 0;
            if (decimal.TryParse(guna2TextBox2.Text, out decimal r))
                received = r;

            label8.Text = $"{received:N2}";
            label17.Text = "المدفوع :";

            // المتبقي على العميل
            decimal remaining = _netTotal - received;
            label12.Text = $"{remaining:N2}";
            label11.Text = "المتبقي علي العميل :";

            // لون المتبقي
            if (remaining > 0)
                label12.ForeColor = Color.FromArgb(239, 68, 68); // أحمر
            else if (remaining < 0)
                label12.ForeColor = Color.FromArgb(45, 204, 113); // أخضر (باقي للعميل)
            else
                label12.ForeColor = Color.White;

            // الرصيد السابق للعميل
            if (SelectedCustomer != null)
            {
                label20.Text = $"{SelectedCustomer.CurrentBalance:N2}";
                label21.Text = "الرصيد السابق :";
            }
            else
            {
                label20.Text = "0.00";
                label21.Text = "الرصيد السابق :";
            }

            // شامل الضريبة 
            label6.Text = $"شامل الضريبة المضافة {SettingsService.TaxPercent}%";
        }

        // ═══════════════════════════════════════════════════════
        //  لوحة الأرقام
        // ═══════════════════════════════════════════════════════
        private void NumPad_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn)
            {
                string digit = btn.Text;

                // منع أكثر من نقطة عشرية
                if (digit == "." && guna2TextBox2.Text.Contains("."))
                    return;

                // لو النص هو صفر بس، نبدله
                if (guna2TextBox2.Text == "0" && digit != ".")
                    guna2TextBox2.Text = digit;
                else
                    guna2TextBox2.Text += digit;
            }
        }

        private void NumPad_Clear(object sender, EventArgs e)
        {
            guna2TextBox2.Text = "";
        }

        private void Backspace_Click(object sender, EventArgs e)
        {
            if (guna2TextBox2.Text.Length > 0)
                guna2TextBox2.Text = guna2TextBox2.Text.Substring(0, guna2TextBox2.Text.Length - 1);
        }

        // ═══════════════════════════════════════════════════════
        //  أزرار المبالغ السريعة
        // ═══════════════════════════════════════════════════════
        private void QuickAmount_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn)
            {
                // استخراج الرقم من النص (مثل "+ 200" → 200)
                string numText = btn.Text.Replace("+", "").Trim();
                if (decimal.TryParse(numText, out decimal amount))
                {
                    decimal current = 0;
                    if (decimal.TryParse(guna2TextBox2.Text, out decimal c))
                        current = c;

                    guna2TextBox2.Text = (current + amount).ToString("0.##");
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  زر المبلغ بالضبط
        // ═══════════════════════════════════════════════════════
        private void ExactAmount_Click(object sender, EventArgs e)
        {
            guna2TextBox2.Text = _netTotal.ToString("0.##");
        }

        // ═══════════════════════════════════════════════════════
        //  تغيير المبلغ المستلم — إعادة الحساب
        // ═══════════════════════════════════════════════════════
        private void ReceivedAmount_TextChanged(object sender, EventArgs e)
        {
            RecalculateAll();
        }

        // ═══════════════════════════════════════════════════════
        //  تغيير الخصم — إعادة الحساب
        // ═══════════════════════════════════════════════════════
        private void Discount_TextChanged(object sender, EventArgs e)
        {
            RecalculateAll();
        }

        // ═══════════════════════════════════════════════════════
        //  أزرار طريقة الدفع
        // ═══════════════════════════════════════════════════════
        private void PaymentMethod_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn)
            {
                // التحقق من الآجل
                if (btn == btnDelivery) // آجل
                {
                    if (SelectedCustomer == null)
                    {
                        ToastManager.ShowWarning("تنبيه", "يجب تحديد عميل أولاً للدفع الآجل");
                        return;
                    }
                    if (!SelectedCustomer.AllowCredit)
                    {
                        ToastManager.ShowWarning("تنبيه", "هذا العميل لا يسمح بالدفع الآجل");
                        return;
                    }
                    _selectedPaymentMethod = 2;
                }
                else if (btn == btnTable) // كاش
                {
                    _selectedPaymentMethod = 1;
                }
                else // نقدي
                {
                    _selectedPaymentMethod = 0;
                }

                UpdatePaymentMethodSelection(btn);
            }
        }

        private void UpdatePaymentMethodSelection(Guna2Button selectedBtn)
        {
            Guna2Button[] buttons = { btnSaffari, btnTable, btnDelivery };
            foreach (var btn in buttons)
            {
                btn.Checked = (btn == selectedBtn);
            }
        }

        private void UpdateCreditButtonState()
        {
            // تفعيل/تعطيل زر الآجل بناءً على وجود عميل
            if (SelectedCustomer == null || !SelectedCustomer.AllowCredit)
            {
                btnDelivery.ForeColor = Color.FromArgb(100, 110, 125);
            }
            else
            {
                btnDelivery.ForeColor = Color.White;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  تأكيد الدفع
        // ═══════════════════════════════════════════════════════
        private void ConfirmPayment_Click(object sender, EventArgs e)
        {
            decimal received = 0;
            decimal.TryParse(guna2TextBox2.Text, out received);

            // التحقق: الدفع الآجل
            if (_selectedPaymentMethod == 2) // آجل
            {
                if (SelectedCustomer == null)
                {
                    ToastManager.ShowWarning("تنبيه", "يجب تحديد عميل أولاً للدفع الآجل");
                    return;
                }

                if (!SelectedCustomer.AllowCredit)
                {
                    ToastManager.ShowWarning("تنبيه", "هذا العميل لا يسمح بالدفع الآجل");
                    return;
                }

                // التحقق من حد الائتمان
                decimal newBalance = SelectedCustomer.CurrentBalance + (_netTotal - received);
                if (newBalance > SelectedCustomer.CreditLimit)
                {
                    ToastManager.ShowError("تجاوز حد الائتمان",
                        $"الرصيد الجديد ({newBalance:N2}) يتجاوز حد الائتمان ({SelectedCustomer.CreditLimit:N2}).\n" +
                        $"الحد المتاح: {(SelectedCustomer.CreditLimit - SelectedCustomer.CurrentBalance):N2} ج");
                    return;
                }
            }
            else // نقدي أو كاش
            {
                if (received < _netTotal)
                {
                    // يسمح بدفع جزئي مع تحذير
                    if (!frmConfirm.Show("دفع جزئي", $"المبلغ المدفوع ({received:N2}) أقل من الإجمالي ({_netTotal:N2}).\nهل تريد المتابعة؟"))
                        return;
                }
            }

            // تعيين النتائج
            PaidAmount = received;
            ChangeAmount = received - _netTotal;
            PaymentMethod = _selectedPaymentMethod;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ═══════════════════════════════════════════════════════
        //  إلغاء
        // ═══════════════════════════════════════════════════════
        private void CancelPayment_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
