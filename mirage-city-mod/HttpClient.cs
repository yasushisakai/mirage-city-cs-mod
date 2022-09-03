using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System;

namespace mirage_city_mod
{
    public class NetworkConstants
    {
        public static readonly string ServerAddress = "http://34.207.233.31/api";

        public static IEnumerator SendJson(string endpoint, object obj, string method)
        {
            var url = $"{NetworkConstants.ServerAddress}{endpoint}";
            Debug.Log(url);
            var req = new UnityWebRequest(url, method);
            var jsonString = JsonUtility.ToJson(obj);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("sending JSON");
            yield return req.Send();
        }

        public static IEnumerator SendPng(string endpoint, byte[] bytes)
        {
            var url = $"{NetworkConstants.ServerAddress}{endpoint}";
            Debug.Log(url);
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "image/png");
            Debug.Log("sending PNG");
            yield return req.Send();
            Debug.Log(req.error);
            yield return null;
        }
    }

    public class HttpClient : MonoBehaviour
    {

        public bool update = false;

        private CityInfo info;
        private CityMetaData meta;

        private bool uploadSS;
        private byte[] screenshotBuffer;

        public HttpClient()
        {
            info = new CityInfo();
            meta = new CityMetaData(Constants.Address);
        }

        public void upload(byte[] bytes)
        {
            Debug.Log("client received bytes to send");
            screenshotBuffer = bytes;
            uploadSS = true;
        }

        public void Start()
        {
            update = false;
            StartCoroutine(RegisterToServer());
            StartCoroutine(UpdateInfo());
        }

        public void Update()
        {
            if (update)
            {
                update = false;
                StartCoroutine(UpdateInfo());
            }

            if (uploadSS && screenshotBuffer.Length != 0)
            {
                uploadSS = false;
                StartCoroutine(UploadScreenshot());
            }

        }

        private IEnumerator UpdateInfo()
        {
            info.update();
            return NetworkConstants.SendJson($"/city/info/{meta.id}", info, "PUT");
        }

        private IEnumerator UploadScreenshot()
        {
            yield return new WaitForSeconds(1);
            var buf = new byte[screenshotBuffer.Length];
            Array.Copy(screenshotBuffer, buf, screenshotBuffer.Length);
            Debug.Log($"sending {buf.Length} bytes");
            screenshotBuffer = new byte[0];
            // yield return NetworkConstants.SendJson($"/city/info/{meta.id}", info, "PUT");
            yield return NetworkConstants.SendPng($"/city/upload/{meta.id}", buf);
        }


        private IEnumerator RegisterToServer()
        {
            Debug.Log("registering city");
            return NetworkConstants.SendJson("/city/register", meta, "POST");
        }
    }
}