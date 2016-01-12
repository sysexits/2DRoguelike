using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPClient : MonoBehaviour {
    public Socket sock;
    IPAddress endpoint_addr;
    IPEndPoint endpoint;

    public void InitiateSocket(string ipAddr, int port = 41234)
    {
        Debug.Log("ip = " + ipAddr);
        if(sock == null)
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endpoint_addr = IPAddress.Parse(ipAddr);
        endpoint = new IPEndPoint(endpoint_addr, port);
    }

    void Update()
    {
    }

    // parse a hash table into a JSON string and send it
    public void sendJSONObject(Hashtable values)
    {
        string JSONString = JSON.JsonEncode(values);

        Debug.Log("sending JSONObject " + JSONString);

        byte[] sendBuffer = Encoding.ASCII.GetBytes(JSONString);
        try
        {
            sock.SendTo(sendBuffer, endpoint);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
