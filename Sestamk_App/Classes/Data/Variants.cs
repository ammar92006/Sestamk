using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sestamk.Classes.Data
{
    public class Variants : INotifyPropertyChanged
    {
        public int VariantID { get; set; }
        public int ProductID { get; set; }

        private string variantCode;
        public string VariantCode
        {
            get => variantCode;
            set
            {
                if (variantCode != value)
                {
                    variantCode = value;
                    OnPropertyChanged(nameof(VariantCode));
                }

            }
        }
        private string variantNameAr;
        public string VariantNameAr
        {
            get => variantNameAr;
            set
            {
                if (variantNameAr != value)
                {
                    variantNameAr = value;
                    OnPropertyChanged(nameof(VariantNameAr));
                }

            }
        }
        private string variantNameEn;
        public string VariantNameEn
        {
            get => variantNameEn;
            set
            {
                if (variantNameEn != value)
                {
                    variantNameEn = value;
                    OnPropertyChanged(nameof(VariantNameEn));
                }

            }
        }
        private string variantImage;
        public string VariantImage
        {
            get => variantImage;
            set
            {
                if (variantImage != value)
                {
                    variantImage = value;
                    OnPropertyChanged(nameof(VariantImage));
                }

            }
        }
        private decimal price;
        public decimal Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged(nameof(Price));
                }

            }
        }
        private decimal cost;
        public decimal Cost
        {
            get => cost;
            set
            {
                if (cost != value)
                {
                    cost = value;
                    OnPropertyChanged(nameof(Cost));
                }

            }
        }
        private string barcode;
        public string Barcode
        {
            get => barcode;
            set
            {
                if (barcode != value)
                {
                    barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }

            }
        }
        private int displayOrder;
        public int DisplayOrder
        {
            get => displayOrder;
            set
            {
                if (displayOrder != value)
                {
                    displayOrder = value;
                    OnPropertyChanged(nameof(DisplayOrder));
                }

            }
        }
        private bool hasAddons;
        public bool HasAddons
        {
            get => hasAddons;
            set
            {
                if (hasAddons != value)
                {
                    hasAddons = value;
                    OnPropertyChanged(nameof(HasAddons));
                }

            }
        }
        private bool isDefault;
        public bool IsDefault
        {
            get => isDefault;
            set
            {
                if (isDefault != value)
                {
                    isDefault = value;
                    OnPropertyChanged(nameof(IsDefault));
                }

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
