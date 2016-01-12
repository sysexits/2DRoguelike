using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPClient : MonoBehaviour {
    public Socket sock;
    public string ipAddress = "143.248.139.70";
    IPAddress endpoint_addr;
    IPEndPoint endpoint;

    void Start()
    {
        if(sock == null)
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endpoint_addr = IPAddress.Parse(ipAddress);
        endpoint = new IPEndPoint(endpoint_addr, 41234);
        
        byte[] send_buffer = Encoding.ASCII.GetBytes("{'test': 1234, 'message': 'hello world'}");
        try
        {
            sock.SendTo(send_buffer, endpoint);
        } catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void Awake()
    {
        if (sock == null)
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    void Update()
    {
    }
}
