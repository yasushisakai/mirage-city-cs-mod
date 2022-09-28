using UnityEngine;
using System.Configuration;

namespace mirage_city_mod
{

    // CityMetaData
    // Information to access the game instance, this should contain information
    // that is updated less frequently
    [System.Serializable]
    public class CityMetaData : System.Object
    {
        [SerializeField]
        string name;

        [SerializeField]
        public string id;

        [SerializeField]
        string address;

        public CityMetaData(SimulationMetaData metaData)
        {
            name = metaData.m_CityName;
            id = metaData.m_gameInstanceIdentifier;
            address = Properties.Settings.Default.address;
        }

        public CityMetaData()
        {
            var metaData = SimulationManager.instance.m_metaData;
            name = metaData.m_CityName;
            id = metaData.m_gameInstanceIdentifier;
            address = Properties.Settings.Default.address;
        }

        public override string ToString()
        {
            return $"name: {name}, id: {id}, address: {address}";
        }
    }
}