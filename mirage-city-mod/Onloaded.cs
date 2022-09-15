using ICities;
using UnityEngine;
using ColossalFramework;

namespace mirage_city_mod
{

    public class OnLoaded : LoadingExtensionBase
    {

        public GameObject mirageCityManager;
        public GameObject mainCamera;
        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("Mirage City Mod Loaded.");

            mirageCityManager = new GameObject("Mirage City Manager");

            // 1. register to the server
            mirageCityManager.AddComponent<Reporter>();

            mirageCityManager.AddComponent<DemandMonitor>();

            // 2. start server and wait for commands
            var server = mirageCityManager.AddComponent<TCPServer>() as TCPServer;
            server.setParent(mirageCityManager);
        }
    }
}