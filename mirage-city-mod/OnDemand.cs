using ICities;
using UnityEngine;
using ColossalFramework;

namespace mirage_city_mod
{

    public class OnDemandChanged : DemandExtensionBase
    {
        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            DemandMonitor.Instance.commercial = originalDemand;
            return base.OnCalculateCommercialDemand(originalDemand);
        }

        public override int OnCalculateResidentialDemand(int originalDemand)
        {
            DemandMonitor.Instance.residential = originalDemand;
            return base.OnCalculateResidentialDemand(originalDemand);
        }

        public override int OnCalculateWorkplaceDemand(int originalDemand)
        {
            DemandMonitor.Instance.industrial = originalDemand;
            return base.OnCalculateWorkplaceDemand(originalDemand);
        }
    }
}