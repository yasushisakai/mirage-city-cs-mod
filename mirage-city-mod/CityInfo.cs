using UnityEngine;
using ICities;
using System;
using Newtonsoft.Json;

namespace mirage_city_mod
{
    public class CityInfo
    {

        public uint elapsed { get; set; }

        public uint population { get; set; }

        public byte happiness { get; set; }

        public int birth_rate { get; set; } // week

        public int death_count { get; set; }

        public CategoryData residential { get; set; }

        public CategoryData industrial { get; set; }

        public CategoryData commercial { get; set; }

        private SimulationMetaData simMetaData;

        private District mainDistrict;

        public CityInfo()
        {
            elapsed = 0;
            population = 0;
            simMetaData = SimulationManager.instance.m_metaData;
            mainDistrict = DistrictManager.instance.m_districts.m_buffer[0];

            happiness = mainDistrict.m_finalHappiness;
            death_count = mainDistrict.GetDeadCount(); // TODO: check DeathAmount??
            birth_rate = (int)mainDistrict.m_birthData.m_finalCount;
            industrial = new CategoryData(mainDistrict.m_industrialData);
            commercial = new CategoryData(mainDistrict.m_commercialData);
            residential = new CategoryData(mainDistrict.m_residentialData);
        }
        public void update()
        {
            var delta = simMetaData.m_currentDateTime - simMetaData.m_startingDateTime;
            elapsed = (uint)delta.TotalSeconds;
            population = mainDistrict.m_populationData.m_finalCount;
            happiness = mainDistrict.m_finalHappiness;
            var demands = DemandMonitor.Instance;
            industrial.update(mainDistrict.m_industrialData, demands.industrial);
            commercial.update(mainDistrict.m_commercialData, demands.commercial);
            residential.update(mainDistrict.m_residentialData, demands.residential);
        }

        public string json()
        {
            return JsonConvert.SerializeObject(this);
            // return JsonUtility.ToJson(this);
        }
        public class CategoryData
        {
            public float happiness { get; set; }

            public float health { get; set; }

            public int demand { get; set; }

            public CategoryData(DistrictPrivateData _dpd)
            {
                happiness = _dpd.m_finalHappiness;
                health = _dpd.m_finalHealth;
                demand = 0;
            }

            public void update(DistrictPrivateData _dpd, int _demand)
            {
                happiness = _dpd.m_finalHappiness;
                health = _dpd.m_finalHealth;
                demand = _demand;
            }
        }
    }


}