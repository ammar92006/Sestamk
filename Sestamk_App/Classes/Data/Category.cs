using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TheArtOfDevHtmlRenderer.Adapters.Entities;
namespace Sestamk.Classes.Data
{
    public class ProductCategories : INotifyPropertyChanged
    {
        public int CategoryID { get; set; }

        private int colorID;
        public int ColorID
        {
            get => colorID;
            set
            {
                if (colorID != value)
                {
                    colorID = value;
                    OnPropertyChanged(nameof(ColorID));
                }
            }
        }
        private string hexCode;
        public string HexCode
        {
            get => hexCode;
            set
            {
                if (hexCode != value)
                {
                    hexCode = value;
                    OnPropertyChanged(nameof(HexCode));
                }
            }
        }
        private int countproducts;
        public int Countproducts
        {
            get => countproducts;
            set
            {
                if (countproducts != value)
                {
                    countproducts = value;
                    OnPropertyChanged(nameof(Countproducts));
                }
            }
        }
        private string categoryCode;
        public string CategoryCode
        {
            get => categoryCode;
            set
            {
                if (categoryCode != value)
                {
                    categoryCode = value;
                    OnPropertyChanged(nameof(CategoryCode));
                }
            }
        }
        private string categoryNameAr;
        public string CategoryNameAr
        {
            get => categoryNameAr;
            set
            {
                if (categoryNameAr != value)
                {
                    categoryNameAr = value;
                    OnPropertyChanged(nameof(CategoryNameAr));
                }
            }
        }
        private string categoryNameEn;
        public string CategoryNameEn
        {
            get => categoryNameEn;
            set
            {
                if (categoryNameEn != value)
                {
                    categoryNameEn = value;
                    OnPropertyChanged(nameof(CategoryNameEn));
                }
            }
        }
        private string backgroundColor;
        public string BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
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
        private string createdBy;
        public string CreatedBy
        {
            get => createdBy;
            set
            {
                if (createdBy != value)
                {
                    createdBy = value;
                    OnPropertyChanged(nameof(CreatedBy));
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
        private int categoryTypeID;
        public int CategoryTypeID
        {
            get => categoryTypeID;
            set
            {
                if (categoryTypeID != value)
                {
                    categoryTypeID = value;
                    OnPropertyChanged(nameof(CategoryTypeID));
                }
            }
        }
        private string categoryTypeName;
        public string CategoryTypeName
        {
            get => categoryTypeName;
            set
            {
                if (categoryTypeName != value)
                {
                    categoryTypeName = value;
                    OnPropertyChanged(nameof(CategoryTypeName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
