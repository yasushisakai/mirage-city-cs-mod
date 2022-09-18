using System;
using UnityEngine;

namespace mirage_city_mod
{
    [Serializable]
    public class CityInfo
    {
        public uint elapsed;

        public uint population;

        public byte happiness;

        public uint death_count;

        public uint birth_rate;

        public int residential_demand;

        public int commercial_demand;

        public int industrial_demand;

        public string network_zone_info;

        private District district;

        private ZoneManager zone;

        private ZoneMonitor zm;

        public CityInfo()
        {
            district = DistrictManager.instance.m_districts.m_buffer[0];
            zone = ZoneManager.instance;
            zm = new ZoneMonitor();
        }

        public bool isDifferent(CityInfo other)
        {
            return (other.elapsed != elapsed &&
            other.population != population &&
            other.happiness != happiness &&
            other.death_count != death_count &&
            other.birth_rate != birth_rate &&
            other.industrial_demand != industrial_demand &&
            other.commercial_demand != commercial_demand &&
            other.residential_demand != residential_demand);
        }

        public void update()
        {
            var meta = SimulationManager.instance.m_metaData;
            elapsed = (uint)(meta.m_currentDateTime - meta.m_startingDateTime).TotalSeconds;
            population = district.m_populationData.m_finalCount;
            happiness = district.m_finalHappiness;
            birth_rate = district.m_birthData.m_finalCount;
            death_count = district.m_deathData.m_finalCount;
            residential_demand = zone.m_residentialDemand;
            commercial_demand = zone.m_commercialDemand;
            industrial_demand = zone.m_workplaceDemand;
            network_zone_info = zm.Info();
        }

        public string json()
        {
            return JsonUtility.ToJson(this);
        }

        public string Serialize()
        {
            var result = "{";
            result += $"\"elapsed\":{elapsed}, \"happiness\":{happiness}, \"population\":{population}, \"death_count\":{death_count}, \"birth_rate\":{birth_rate}, ";
            result += $"\"industrial_demand\":{industrial_demand}, \"commercial_demand\":{commercial_demand}, \"residential_demand\": {residential_demand}, ";
            result += $"\"network_zone_info\": {zm.Info()}";
            result += "}";
            return result;
        }
    }
}