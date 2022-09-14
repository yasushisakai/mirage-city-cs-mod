using UnityEngine;
using ICities;

namespace mirage_city_mod
{
    [System.Serializable]
    public class CityInfo : System.Object
    {
        [SerializeField]
        public uint elapsed;

        [SerializeField]
        public uint population;
        public CityInfo()
        {
            elapsed = 0;
            population = 0;
        }
        public void update()
        {
            var metaData = SimulationManager.instance.m_metaData;
            var delta = metaData.m_currentDateTime - metaData.m_startingDateTime;
            elapsed = (uint)delta.TotalSeconds;
            population = DistrictManager.instance.m_districts.m_buffer[0].m_populationData.m_finalCount;
        }
    }
}