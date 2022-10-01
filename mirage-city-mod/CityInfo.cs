using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace mirage_city_mod
{
    [Serializable]
    public class CityInfo
    {

        public static CityInfo Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new CityInfo();
                }
                return _instance;
            }
        }

        private static CityInfo _instance = null;

        public uint elapsed;

        public uint population;

        public byte happiness;

        public uint death_count;

        public uint birth_rate;

        public int residential_demand;

        public int commercial_demand;

        public int industrial_demand;

        public string network_zone_info;

        public Dictionary<String, Scene> scenes;

        private District district;

        private ZoneManager zone;

        private ZoneMonitor zm;

        private int simCounter;

        public static uint GetElapsed()
        {
            var meta = SimulationManager.instance.m_metaData;
            return (uint)(meta.m_currentDateTime - meta.m_startingDateTime).TotalSeconds;
        }

        public CityInfo()
        {
            district = DistrictManager.instance.m_districts.m_buffer[0];
            zone = ZoneManager.instance;
            zm = new ZoneMonitor();
            scenes = new Dictionary<String, Scene>();
            var origin = Scene.Origin();
            scenes.Add("default", origin);
            var close_up = new Scene(900, 200, 400, 90, 90);
            scenes.Add("closeup", close_up);
            simCounter = 0;
            update();
        }

        public void AddScene(string key, Scene value)
        {
            scenes.Add(key, value);
        }

        public void DeleteScene(string key)
        {
            if (scenes.ContainsKey(key))
            {
                scenes.Remove(key);
            }
        }

        public bool isStale()
        {
            var metaData = SimulationManager.instance.m_metaData;
            var newElapsed = (uint)(metaData.m_currentDateTime - metaData.m_startingDateTime).TotalSeconds;
            return newElapsed != elapsed;
        }

        public void update()
        {
            var meta = SimulationManager.instance.m_metaData;
            district = DistrictManager.instance.m_districts.m_buffer[0];
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

        public void incSimCounter()
        {
            simCounter++;
        }

        public void decSimCounter()
        {
            if (simCounter > 0)
                simCounter--;
        }

        public bool ShouldRunSim()
        {
            return simCounter > 0;
        }

        private string serializeSceneNames()
        {
            var sceneKeys = String.Join(",", scenes.Keys.Select(s => "\"" + s + "\"").ToArray());
            return $"[{sceneKeys}]";
        }

        public string Serialize()
        {
            var result = "{";
            result += $"\"elapsed\":{elapsed}, \"happiness\":{happiness}, \"population\":{population}, \"death_count\":{death_count}, \"birth_rate\":{birth_rate}, ";
            result += $"\"industrial_demand\":{industrial_demand}, \"commercial_demand\":{commercial_demand}, \"residential_demand\": {residential_demand}, ";
            result += $"\"scenes\":{serializeSceneNames()}, ";
            result += $"\"network_zone_info\": {zm.Info()}";
            result += "}";
            return result;
        }
    }
}