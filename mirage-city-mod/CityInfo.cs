using UnityEngine;
using ICities;

namespace mirage_city_mod
{
    [System.Serializable]
    public class CityInfo : System.Object
    {
        [SerializeField]
        double elapsed;

        [SerializeField]
        uint population;

        [SerializeField]
        bool simrunning;

        public CityInfo()
        {
            update();
        }
        public void update()
        {
            var metaData = SimulationManager.instance.m_metaData;
            var delta = metaData.m_currentDateTime - metaData.m_startingDateTime;
            elapsed = delta.TotalSeconds;
            population = DistrictManager.instance.m_districts.m_buffer[0].m_populationData.m_finalCount;
            simrunning = !SimulationManager.instance.SimulationPaused;
        }
    }
}