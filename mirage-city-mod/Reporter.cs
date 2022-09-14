using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System;

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
        private static readonly WaitForSeconds updateInterval = new WaitForSeconds(20);
        private static readonly WaitForSeconds printCheckInterval = new WaitForSeconds(1);
        private static HealthCheck hc = new HealthCheck();
        private static CityInfo info = new CityInfo();
        private static PrintScreen printScreen = new PrintScreen(3840, 2160);

        public void Start()
        {
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
            }
        }

        private IEnumerator uploadScreenshot(double _elapsed)
        {

            var endpoint = $"{mirageCityServerAddress}/city/upload/{meta.id}/{(int)_elapsed}";
            Debug.Log($"image upload endpoint: {endpoint}");
            StartCoroutine(printScreen.Shoot());

            while (!printScreen.ready)
            {
                yield return printCheckInterval;
            }

            printScreen.ready = false;
            Debug.Log("Reporter: uploadScreenshot");
            Debug.Log($"printScreenBufferSize: {printScreen.buffer.Length}");
            StartCoroutine(sendJpg(endpoint, printScreen.buffer, "POST"));
        }

        private IEnumerator updateInfo()
        {
            while (true)
            {
                yield return updateInterval;
                var lastElapsed = info.elapsed;
                info.update();
                // did the simulation run?
                // if (lastElapsed != info.elapsed)
                // {
                Debug.Log("Reporter: update");
                StartCoroutine(sendJson(infoUpdateEndpoint, info, "POST"));
                StartCoroutine(uploadScreenshot(info.elapsed));
                // }
            }
        }

        private static UnityWebRequest prepareRequest(string url, byte[] bytes, string method)
        {
            Debug.Log(url);
            var req = new UnityWebRequest(url, method);
            Debug.Log(req.url);
            req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            return req;
        }

        private static IEnumerator sendJson(string endpoint, object obj, string method)
        {
            var jsonString = JsonUtility.ToJson(obj);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var req = prepareRequest(endpoint, bytes, method);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.Send();

            if (req.responseCode != 200)
            {
                Debug.Log($"sending json errored with status code: {req.responseCode}");
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