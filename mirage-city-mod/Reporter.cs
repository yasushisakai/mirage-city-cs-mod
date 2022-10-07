using UnityEngine;
using UnityEngine.Networking;
using ICities;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ColossalFramework.UI;

namespace mirage_city_mod
{

    // this class is responsible for reporting back to the mirage city server.
    public class Reporter : MonoBehaviour
    {
        private static readonly string mirageCityServerAddress = "https://api.mirage.city";
        private static readonly CityMetaData meta = new CityMetaData();
        private static readonly string registerEndpoint = $"{mirageCityServerAddress}/city/register";
        private static readonly string healthCheckEndpoint = $"{mirageCityServerAddress}/city/health_check";
        private static readonly string infoUpdateEndpoint = $"{mirageCityServerAddress}/city/update/{meta.id}";
        private static readonly string commitIdChangedEndpoint = $"{mirageCityServerAddress}/city/command/{meta.id}/commit_id";
        private static readonly string citizenReportEndpoint = $"{mirageCityServerAddress}/city/push_citizen/{meta.id}";

        private static readonly string saveFolderDirectory = "C:\\Users\\yasushi\\AppData\\Local\\Colossal Order\\Cities_Skylines\\Saves";
        public static readonly int healthCheckIntervalSeconds = 5;
        private static readonly WaitForSeconds healthCheckInterval = new WaitForSeconds(healthCheckIntervalSeconds);
        private static readonly WaitForEndOfFrame imageCheckInterval = new WaitForEndOfFrame();
        private static readonly WaitForSeconds updateInterval = new WaitForSeconds(30);

        // note that there is a Task Scheduler (like a cron job) in the OS side to auto commit the git repository. 
        private static readonly WaitForSeconds saveInterval = new WaitForSeconds(60 * 15); // 15min 
        private HealthCheck hc;
        private CamController camCon;

        private string commitId;
        public void Start()
        {

            hc = new HealthCheck();
            camCon = GetComponent<CamController>();
            commitId = getCommitId();
            StartCoroutine(register());
            StartCoroutine(healthCheck());
            StartCoroutine(updateInfo());
            StartCoroutine(archiveCities());
        }

        public class CommitIdChange
        {
            public uint timestamp;
            public string was;
            public string next;

            public CommitIdChange(uint _time, string _was, string _next)
            {
                timestamp = _time;
                was = _was;
                next = _next;
            }
        }


        private IEnumerator register()
        {
            var commitId = getCommitId();

            yield return sendJson(registerEndpoint, meta, "POST");
        }

        private IEnumerator healthCheck()
        {
            while (true)
            {
                yield return healthCheckInterval;
                hc.update();
                yield return sendJson(healthCheckEndpoint, hc, "POST");
                if (CityInfo.Instance.ShouldRunSim())
                {
                    SimulationManager.instance.SimulationPaused = false;
                    CityInfo.Instance.decSimCounter();
                }
                else
                {
                    if (!SimulationManager.instance.SimulationPaused)
                    {
                        SimulationManager.instance.SimulationPaused = true;
                    }
                }

                if (commitId != getCommitId())
                {
                    var newCommitId = getCommitId();
                    var commitChanged = new CommitIdChange(CityInfo.Instance.elapsed, commitId, newCommitId);
                    yield return sendJson(commitIdChangedEndpoint, commitChanged, "POST");
                    commitId = newCommitId;
                }

            }
        }

        private IEnumerator uploadScreenshots(double _elapsed, IDictionary<string, Scene> scenes)
        {
            camCon.setFreeCamera();
            foreach (KeyValuePair<string, Scene> s in scenes)
            {
                yield return camCon.SetScene(s.Value);
                yield return uploadScreenshot(_elapsed, s.Key);
            }
            camCon.disableFreeCamera();
            yield return null;
        }

        private IEnumerator uploadScreenshot(double _elapsed, string key)
        {

            var endpoint = $"{mirageCityServerAddress}/city/upload/{meta.id}/{(int)_elapsed}/{key}";
            var screen = PrintScreen.Instance;
            yield return screen.Shoot();
            while (!screen.ready)
            {
                yield return imageCheckInterval;
            }
            yield return sendJpg(endpoint, screen.buffer, "POST");
            screen.Reset();
        }

        private IEnumerator updateInfo()
        {
            var info = CityInfo.Instance;
            yield return uploadScreenshots(info.elapsed, info.scenes);
            yield return sendText(infoUpdateEndpoint, info.Serialize(), "POST");

            while (true)
            {
                yield return updateInterval;
                if (info.isStale())
                {
                    info.update();
                    Debug.Log("--- updating city info ---");
                    // screen shots are based on the same info
                    yield return reportCitizen();
                    yield return uploadScreenshots(info.elapsed, info.scenes);
                    yield return sendText(infoUpdateEndpoint, info.Serialize(), "POST");
                }
            }
        }

        private IEnumerator reportCitizen()
        {
            CitizenData cd;
            if (CitizenFinder.Instance.FindCitizen(out cd))
            {
                yield return sendJson(citizenReportEndpoint, cd, "POST");
            }
            yield return null;
        }

        private IEnumerator archiveCities()
        {
            while (true)
            {
                yield return saveInterval;
                SavePanel savePanel = UIView.library.Get<SavePanel>("SavePanel");
                if (savePanel != null)
                {
                    Debug.Log("--- saving game file ---");
                    var gameName = SimulationManager.instance;
                    var metaData = SimulationManager.instance.m_metaData;
                    name = metaData.m_CityName;
                    savePanel.SaveGame(name); // the save directory is a git repository
                }
            }
        }

        private string getCommitId()
        {
            var main_branch_ref_head_file = $"{Reporter.saveFolderDirectory}//.git//refs//heads//main";
            var commitId = File.ReadAllText(main_branch_ref_head_file, Encoding.UTF8);
            return commitId.Trim();
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
            // Debug.Log(message);
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