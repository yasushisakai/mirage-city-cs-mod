using UnityEngine;

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
        string map;

        [SerializeField]
        public string id;

        [SerializeField]
        string address;

        public CityMetaData(SimulationMetaData metaData, string _address)
        {
            name = metaData.m_CityName;
            map = metaData.m_MapName;
            id = metaData.m_gameInstanceIdentifier;
            address = _address;
        }

        public CityMetaData(string _address)
        {
            var metaData = SimulationManager.instance.m_metaData;
            name = metaData.m_CityName;
            map = metaData.m_MapName;
            id = metaData.m_gameInstanceIdentifier;
            address = _address;
        }

        public override string ToString()
        {
            return $"name: {name}, mapName: {map}, id: {id}, address: {address}";
        }
    }
}