using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

namespace mirage_city_mod
{

    // this class is responsible for reporting back to the mirage city server.
    public class Reporter : MonoBehaviour
    {
        private static readonly string mirageCityServerAddress = "https://api.mirage.city";
        private static readonly string myAddress = "18.27.123.81:9999";
        private static readonly CityMetaData meta = new CityMetaData(myAddress);
        private static readonly string registerEndpoint = $"{mirageCityServerAddress}/city/register";
        private static readonly string healthCheckEndpoint = $"{mirageCityServerAddress}/city/health_check";
        private static readonly string infoUpdateEndpoint = $"{mirageCityServerAddress}/city/update/{meta.id}";
        private static readonly WaitForSeconds healthCheckInterval = new WaitForSeconds(10);
        private static readonly WaitForSeconds updateInterval = new WaitForSeconds(15);
        private static readonly WaitForSeconds printCheckInterval = new WaitForSeconds(1);
        private HealthCheck hc;
        private CityInfo info;
        private PrintScreen printScreen = new PrintScreen(3840, 2160);

        public void Start()
        {

            hc = new HealthCheck();
            info = new CityInfo();
            printScreen = new PrintScreen(3840, 2160);

            StartCoroutine(register());
            StartCoroutine(healthCheck());
            StartCoroutine(updateInfo());
        }

        private IEnumerator register()
        {
            return sendJson(registerEndpoint, meta, "POST");
        }

        private IEnumerator healthCheck()
        {
            while (true)
            {
                yield return healthCheckInterval;
                hc.update();
                StartCoroutine(sendJson(healthCheckEndpoint, hc, "POST"));
                var zm = new ZoneMonitor();
                // zm.checkZoneblocks();
            }
        }

        private IEnumerator uploadScreenshot(double _elapsed)
        {

            var endpoint = $"{mirageCityServerAddress}/city/upload/{meta.id}/{(int)_elapsed}";
            StartCoroutine(printScreen.Shoot());

            while (!printScreen.ready)
            {
                yield return printCheckInterval;
            }

            printScreen.ready = false;
            StartCoroutine(sendJpg(endpoint, printScreen.buffer, "POST"));
        }

        private IEnumerator updateInfo()
        {
            while (true)
            {
                yield return updateInterval;
                var new_info = new CityInfo();
                new_info.update();
                if (new_info.isDifferent(info))
                {
                    StartCoroutine(sendText(infoUpdateEndpoint, info.Serialize(), "POST"));
                    StartCoroutine(uploadScreenshot(info.elapsed));
                    info = new_info;
                }
            }
        }

        private static UnityWebRequest prepareRequest(string url, byte[] bytes, string method)
        {
            var req = new UnityWebRequest(url, method);
            req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            return req;
        }

        private static IEnumerator sendText(string endpoint, string message, string method)
        {
            Debug.Log(message);
            var bytes = Encoding.UTF8.GetBytes(message);
            var req = prepareRequest(endpoint, bytes, method);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.Send();

            if (req.responseCode != 200)
            {
                Debug.Log($"sending json errored with status code: {req.responseCode}");
                Debug.Log($"{req.downloadHandler.text}");
            }

            yield return null;
        }

        private static IEnumerator sendJson(string endpoint, object obj, string method)
        {
            var jsonString = JsonUtility.ToJson(obj);
            Debug.Log(jsonString);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var req = prepareRequest(endpoint, bytes, method);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.Send();

            if (req.responseCode != 200)
            {
                Debug.Log($"sending json errored with status code: {req.responseCode}");
                Debug.Log($"{req.downloadHandler.text}");
            }

            yield return null;
        }

        private static IEnumerator sendJpg(string endpoint, byte[] bytes, string method)
        {
            var req = prepareRequest(endpoint, bytes, method);
            req.SetRequestHeader("Content-Type", "image/jpeg");
            req.timeout = 10; // seconds

            yield return req.Send();

            if (req.responseCode != 200)
            {
                Debug.Log($"sending image errored with: {req.responseCode}");
            }
            yield return null;
        }

    }

    public class HealthCheck
    {
        public bool is_sim_running;
        public string id;

        public HealthCheck()
        {
            var sim = SimulationManager.instance;
            is_sim_running = !sim.SimulationPaused;
            id = sim.m_metaData.m_gameInstanceIdentifier;
        }

        public void update()
        {
            var sim = SimulationManager.instance;
            is_sim_running = !sim.SimulationPaused;
        }
    }

}