using System;
using System.Collections.Generic;
using System.Text;

namespace Sestamk.Classes
{
    public class LicenseModel
    {
        public Guid CompanyId { get; set; }
        public string SubscriptionPlan { get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public int MaxDevices { get; set; }

        public string DeviceFingerprint { get; set; }

        public DateTime LastOnlineCheck { get; set; }
        public int MaxOfflineDays { get; set; }

        public string Signature { get; set; }

        public static void RefreshLicenseFromServer (LicenseModel License)
        {

        }
    }
}
