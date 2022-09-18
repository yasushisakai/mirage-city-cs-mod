using System;
using UnityEngine;

namespace mirage_city_mod
{
    [Serializable]
    public class CityInfo
    {
        public uint elapsed;

        public byte happiness;

        public uint death_count;

        public uint birth_rate;

        public int residential_demand;

        public int commercial_demand;

        public int industrial_demand;

        public string zone_info;

        private District district;

        private ZoneManager zone;

        private ZoneMonitor zm;

        public CityInfo()
        {
            district = DistrictManager.instance.m_districts.m_buffer[0];
            zone = ZoneManager.instance;
            zm = new ZoneMonitor();
        }

        public void update()
        {
            var meta = SimulationManager.instance.m_metaData;
            elapsed = (uint)(meta.m_currentDateTime - meta.m_startingDateTime).TotalSeconds;
            happiness = district.m_finalHappiness;
            birth_rate = district.m_birthData.m_finalCount;
            death_count = district.m_deathData.m_finalCount;
            residential_demand = zone.m_residentialDemand;
            commercial_demand = zone.m_commercialDemand;
            industrial_demand = zone.m_workplaceDemand;
            zone_info = zm.Info();
        }

        public string json()
        {
            return JsonUtility.ToJson(this);
        }
    }
}