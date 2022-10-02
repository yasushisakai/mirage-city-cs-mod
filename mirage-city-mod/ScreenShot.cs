using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;
using ColossalFramework.Importers;
using ColossalFramework.Threading;

namespace mirage_city_mod
{

    public class PrintScreen
    {

        public static PrintScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PrintScreen();
                }
                return _instance;
            }
        }

        private static PrintScreen _instance = null;

        public byte[] buffer;
        public bool ready;
        public int width = 640;
        public int height = 480;

        public PrintScreen(int _width = 2880, int _height = 1620)
        {
            width = _width;
            height = _height;
            ready = false;
        }

        public void Reset()
        {
            ready = false;
            buffer = new byte[0];
        }

        public IEnumerator Shoot()
        {
            Debug.Log("taking screen shot");
            // take screenshot and save it in buffer
            yield return new WaitForEndOfFrame();
            int upscaleWidth = width * 4;
            int upscaleHeight = height * 4;
            int skip = 0;
            if ((float)width / (float)height > Camera.main.aspect)
            {
                upscaleHeight = Mathf.CeilToInt((float)upscaleWidth / Camera.main.aspect);
                skip = (upscaleHeight - 4 * height) / 2;
            }
            RenderManager.instance.RequiredAspect = (float)upscaleWidth / (float)upscaleHeight;
            yield return new WaitForEndOfFrame();
            RenderManager.instance.RequiredAspect = 0f;
            RenderTexture rtHDR = new RenderTexture(upscaleWidth, upscaleHeight, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            RenderTexture rtLDR = new RenderTexture(upscaleWidth, upscaleHeight, 0);
            Camera.main.targetTexture = rtHDR;
            Rect originalCamRect = Camera.main.rect;
            Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
            Texture2D screenShot = new Texture2D(upscaleWidth, upscaleHeight - 2 * skip, TextureFormat.ARGB32, mipmap: false);
            bool smaaEnabled = false;
            SMAA smaa = Camera.main.GetComponent<SMAA>();
            if (smaa != null)
            {
                smaaEnabled = smaa.enabled;
                smaa.enabled = false;
            }
            OverlayEffect overlay = Camera.main.GetComponent<OverlayEffect>();
            RenderManager.instance.UpdateCameraInfo();
            Camera.main.Render();
            if (smaa != null)
            {
                smaa.enabled = smaaEnabled;
            }
            Graphics.Blit(rtHDR, rtLDR);
            RenderTexture.active = rtLDR;
            screenShot.ReadPixels(new Rect(0f, skip, upscaleWidth, upscaleHeight - 2 * skip), 0, 0);
            Camera.main.targetTexture = null;
            Camera.main.rect = originalCamRect;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(rtHDR);
            UnityEngine.Object.Destroy(rtLDR);
            int sw = screenShot.width;
            int sh = screenShot.height;
            Color32[] sc = screenShot.GetPixels32();
            Debug.Log("dispating thread for saving image buffer");
            ThreadHelper.taskDistributor.Dispatch(delegate
            {
                Image image = new Image(sw, sh, TextureFormat.RGB24, sc);
                image.Resize(width, height);
                if (SimulationManager.exists)
                {
                    buffer = image.GetFormattedImage(Image.BufferFileFormat.JPG);
                    ready = true;

                    ThreadHelper.dispatcher.Dispatch(delegate
                    {
                        UnityEngine.Object.Destroy(screenShot);
                        if (overlay != null)
                        {
                            overlay.m_lastScreenshotTimestamp = DateTime.Now;
                        }

                    });
                }
            }
            );
        }

    }
}