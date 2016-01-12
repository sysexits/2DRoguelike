using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;

public class UDPListener : MonoBehaviour
{
    private const int listenPort = 12345;

    UdpClient listener;
    IPEndPoint groupEP;
    Thread listeningThread;
    bool listenerRunning = false;
    
    void Start()
    {
        listener = new UdpClient(listenPort);
        groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        listenerRunning = true;

        listeningThread = new Thread(Run);
        listeningThread.Start();
    }

    void OnDestroy()
    {
        listenerRunning = false;
        listeningThread.Join();
        listener.Close();
    }
	
	void Run()
    {
        while (listenerRunning)
        {
            string received_data;
            byte[] receive_byte_array;
            try
            {
                receive_byte_array = listener.Receive(ref groupEP);
                Debug.Log("Received a broadcast from " + groupEP.ToString());
                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                Debug.Log("data follows " + received_data);

                Hashtable data = (Hashtable)JSON.JsonDecode(receive_byte_array);
                switch (data["action"].ToString())
                {
                    case "myinfo":
                        GetComponent<Roguelike.BoardManager>().generateEnemy(
                            System.Int32.Parse(data["posx"].ToString()),
                            System.Int32.Parse(data["posy"].ToString())
                        );
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}