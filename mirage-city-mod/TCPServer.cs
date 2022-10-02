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
        private GameObject _parent;
        private Reporter _reporter;

        // the mod will listen to this port.
        private int port = 44445;
        private TcpClient client;

        public void Start()
        {
            listenerThread = new Thread(new ThreadStart(ListenLoop));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        public void SetParent(GameObject parent)
        {
            _parent = parent;
            _reporter = parent.GetComponent<Reporter>();
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
                    var gotCommand = false;
                    while ((len = stream.Read(buffer, 0, buffer.Length)) != 0 && stream.CanRead)
                    {
                        Debug.Log($"read {len} bytes.");
                        var incoming = new byte[len];
                        Array.Copy(buffer, 0, incoming, 0, len);
                        string mes = Encoding.UTF8.GetString(incoming);
                        Debug.Log($"incoming mes: {mes}");
                        var command = mes[0];
                        var elapsed = CityInfo.GetElapsed();
                        switch (command)
                        {
                            case 'h':
                                SendMessage(stream, $"OK,{elapsed}");
                                gotCommand = true;
                                break;
                            case 't':
                                SimulationManager.instance.SimulationPaused = !SimulationManager.instance.SimulationPaused;
                                SendMessage(stream, $"OK,{elapsed}");
                                Debug.Log("toggle done.");
                                gotCommand = true;
                                break;
                            case 's':
                                addScene(mes);
                                Debug.Log("added scene");
                                SendMessage(stream, $"OK,{elapsed}");
                                gotCommand = true;
                                break;
                            case 'd':
                                deleteScene(mes);
                                Debug.Log("deleted scene");
                                SendMessage(stream, $"OK,{elapsed}");
                                gotCommand = true;
                                break;
                            case 'z':
                                changeZone(mes);
                                CityInfo.Instance.addSimDurationFor(60);
                                Debug.Log($"zonning done.");
                                SendMessage(stream, $"OK,{elapsed}");
                                gotCommand = true;
                                break;
                            case 'e':
                                Debug.Log("empty.");
                                CityInfo.Instance.addSimDurationFor(60);
                                SendMessage(stream, $"OK,{elapsed}");
                                gotCommand = true;
                                break;
                            default:
                                SendMessage(stream, "Invalid Command");
                                break;
                        }

                        if (gotCommand)
                        {
                            break;
                        }
                    }
                    Debug.Log("closing stream");
                    stream.Close();
                    Debug.Log("closing client");
                    client.Close();
                    client = null;
                }
            }
            catch (SocketException e)
            {
                Debug.Log("Mirage City Mod Error: " + e.ToString());
            }
        }

        private void changeZone(string message)
        {
            var split = message.Split(',');
            var id = UInt16.Parse(split[1]);
            var xzPairNum = (split.Length - 2 / 3);
            for (int i = 2; i < xzPairNum; i += 3)
            {
                var x = UInt16.Parse(split[i]);
                var z = UInt16.Parse(split[i + 1]);
                var zoneId = UInt16.Parse(split[i + 2]);
                Debug.Log($"id: {id}, x: {x}, z: {z}, zone: {zoneId}");
                var newZone = Cell.IdtoZone(zoneId);
                var zone = ZoneMonitor.ChangeLandUse(id, x, z, newZone);
                Debug.Log($"change zone result: {zone}");
            }
        }

        private void addScene(string message)
        {
            var split = message.Split(',');
            // you need a name, x, z, size, yaw, pitch;
            var name = split[1].Trim();
            var x = float.Parse(split[2]);
            var z = float.Parse(split[3]);
            var size = float.Parse(split[4]);
            var yaw = float.Parse(split[5]);
            var pitch = float.Parse(split[6]);
            var scene = new Scene(x, z, size, yaw, pitch);
            CityInfo.Instance.AddScene(name, scene);
        }

        private void deleteScene(string message)
        {
            var split = message.Split(',');
            var name = split[1].Trim();
            CityInfo.Instance.DeleteScene(name);
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