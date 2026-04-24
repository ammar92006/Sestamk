using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sestamk.Classes.Data
{
    public class Products : INotifyPropertyChanged
    {
        public int ProductID { get; set; }

        private string productCode;
        public string ProductCode
        {
            get => productCode;
            set
            {
                if (productCode != value)
                {
                    productCode = value;
                    OnPropertyChanged(nameof(ProductCode));
                }
            }
        }
        
        private string productNameAr;
        public string ProductNameAr
        {
            get => productNameAr;
            set
            {
                if (productNameAr != value)
                {
                    productNameAr = value;
                    OnPropertyChanged(nameof(ProductNameAr));
                }
            }
        }
        private string productNameEn;
        public string ProductNameEn
        {
            get => productNameEn;
            set
            {
                if (productNameEn != value)
                {
                    productNameEn = value;
                    OnPropertyChanged(nameof(ProductNameEn));
                }
            }
        }
        private int categoryID;
        public int CategoryID
        {
            get => categoryID;
            set
            {
                if (categoryID != value)
                {
                    categoryID = value;
                    OnPropertyChanged(nameof(CategoryID));
                }
            }
        }
        private string image;
        public string Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }
        private string description;
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        private float discountPercent;
        public float DiscountPercent
        {
            get => discountPercent;
            set
            {
                if (discountPercent != value)
                {
                    discountPercent = value;
                    OnPropertyChanged(nameof(DiscountPercent));
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
        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }
        private string preparationTime;
        public string PreparationTime
        {
            get => preparationTime;
            set
            {
                if (preparationTime != value)
                {
                    preparationTime = value;
                    OnPropertyChanged(nameof(PreparationTime));
                }
            }
        }
        private string notes;
        public string Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }
        private string createdDate;
        public string CreatedDate
        {
            get => createdDate;
            set
            {
                if (createdDate != value)
                {
                    createdDate = value;
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
        }
        private int createdByUserID;
        public int CreatedByUserID
        {
            get => createdByUserID;
            set
            {
                if (createdByUserID != value)
                {
                    createdByUserID = value;
                    OnPropertyChanged(nameof(CreatedByUserID));
                }
            }
        }
        private string lastModified;
        public string LastModified
        {
            get => lastModified;
            set
            {
                if (lastModified != value)
                {
                    lastModified = value;
                    OnPropertyChanged(nameof(LastModified));
                }
            }
        }





        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
