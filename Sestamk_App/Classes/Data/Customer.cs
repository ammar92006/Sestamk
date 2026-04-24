using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace Sestamk.Classes.Data
{
    public class Customer : INotifyPropertyChanged
    {
        public int CustomerID { get; set; }

        private string customerCode;
        public string CustomerCode
        {
            get => customerCode;
            set
            {
                if (customerCode != value)
                {
                    customerCode = value;
                    OnPropertyChanged(nameof(CustomerCode));
                }
            }
        }
        private string customerName;
        public string CustomerName
        {
            get => customerName;
            set
            {
                if (customerName != value)
                {
                    customerName = value;
                    OnPropertyChanged(nameof(CustomerName));
                }
            }
        }

        private string phone1;
        public string Phone1
        {
            get => phone1;
            set
            {
                if (phone1 != value)
                {
                    phone1 = value;
                    OnPropertyChanged(nameof(Phone1));
                }
            }
        }

        private string phone2;
        public string Phone2
        {
            get => phone2;
            set
            {
                if (phone2 != value)
                {
                    phone2 = value;
                    OnPropertyChanged(nameof(Phone2));
                }
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                if (email != value)
                {
                    email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string address;
        public string Address
        {
            get => address;
            set
            {
                if (address != value)
                {
                    address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }
        private decimal currentBalance;
        public decimal CurrentBalance
        {
            get => currentBalance;
            set
            {
                if (currentBalance != value)
                {
                    currentBalance = value;
                    OnPropertyChanged(nameof(CurrentBalance));
                }
            }
        }
        private bool allowCredit;
        public bool AllowCredit
        {
            get => allowCredit;
            set
            {
                if (allowCredit != value)
                {
                    allowCredit = value;
                    OnPropertyChanged(nameof(AllowCredit));
                }
            }
        }
        private decimal creditLimit;
        public decimal CreditLimit
        {
            get => creditLimit;
            set
            {
                if (creditLimit != value)
                {
                    creditLimit = value;
                    OnPropertyChanged(nameof(CreditLimit));
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
        //private bool isDeleted;
        //public bool IsDeleted
        //{
        //    get => isDeleted;
        //    set
        //    {
        //        if (isDeleted != value)
        //        {
        //            isDeleted = value;
        //            OnPropertyChanged(nameof(IsDeleted));
        //        }
        //    }
        //}
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
        private string stopReason;
        public string StopReason
        {
            get => stopReason;
            set
            {
                if (stopReason != value)
                {
                    stopReason = value;
                    OnPropertyChanged(nameof(StopReason));
                }
            }
        }
        private int rating;
        public int Rating
        {
            get => rating;
            set
            {
                if (rating != value)
                {
                    rating = value;
                    OnPropertyChanged(nameof(Rating));
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
        private string lastTransactionDate;
        public string LastTransactionDate
        {
            get => lastTransactionDate;
            set
            {
                if (lastTransactionDate != value)
                {
                    lastTransactionDate = value;
                    OnPropertyChanged(nameof(LastTransactionDate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
