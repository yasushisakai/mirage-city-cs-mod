using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using ICities;

namespace mirage_city_mod
{

    /// rudimentary TCP Server
    /// This is only intended to have a single connection at a time.
    /// mainly the server will send commands to to this game instance.
    public class TCPServer : MonoBehaviour
    {

        private Thread listenerThread;
        private TcpListener listener;

        private GameObject parent;

        // the mod will listen to this port.
        private int port = 44445;
        private TcpClient client;

        public void Start()
        {
            listenerThread = new Thread(new ThreadStart(ListenLoop));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        public void setParent(GameObject _parent)
        {
            parent = _parent;
        }

        private void ListenLoop()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Debug.Log("listening in port: " + port);
                while (true)
                {
                    Debug.Log("Wating for connection: ");
                    // this is a blocking function
                    client = listener.AcceptTcpClient();
                    Debug.Log("Connected to client");
                    NetworkStream stream = client.GetStream();
                    int len;
                    byte[] buffer = new byte[1024];
                    while ((len = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        var incoming = new byte[len];
                        Array.Copy(buffer, 0, incoming, 0, len);
                        string mes = Encoding.UTF8.GetString(incoming);
                        Debug.Log($"mes: {mes}");
                        switch (mes)
                        {
                            case "hello":
                                SendMessage(stream, "hi");
                                break;
                            case "toggle_sim":
                                SimulationManager.instance.SimulationPaused = !SimulationManager.instance.SimulationPaused;
                                SendMessage(stream, "OK");
                                break;
                            case "update_info":
                                var client = parent.GetComponent<HttpClient>();
                                client.update = true;
                                SendMessage(stream, "OK");
                                break;
                            case "upload_screen_shot":
                                var screenshot = parent.GetComponent<ScreenShot>();
                                screenshot.trigger();
                                SendMessage(stream, "OK");
                                break;
                            default:
                                SendMessage(stream, "Invalid Command");
                                break;
                        }
                    }
                    client.Close();
                    client = null;
                }
            }
            catch (SocketException e)
            {
                Debug.Log("Mirage City Mod Error: " + e.ToString());
            }
        }

        private void SendMessage(NetworkStream stream, string mes)
        {
            if (stream.CanWrite)
            {
                var bytes = Encoding.UTF8.GetBytes(mes);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }


}