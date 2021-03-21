// Discover "siblings" on the local network (other computers running the same app).
// Figure out some properties about each (eg. network latency)

using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using Google.Protobuf.Network;
using Google.Protobuf;

// TODO: This is using Unity's Time.time for now, but we should probably switch to a
// more precise time measurement later. Otherwise, we'll start losing milisecond precision
// after a few hours running Time.time is a float

class Sibling {
    public string ip;
    public float lastPing;
    public double latency;
}
public class Network_Discovery : MonoBehaviour
{
    // settings
    public int port = 9876;
    public float siblingMaxStale = 5;
    public float pingInterval = 2;

    // Network client
    private UdpClient udpClient;
    private IPEndPoint from;

    // status
    private float lastBroadcastTime = 0;
    private string localIp = "0.0.0.0";
    private Dictionary<string, Sibling> siblings = new Dictionary<string, Sibling>();
    bool waitingUDP = false;

    void Start() {
        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

        from = new IPEndPoint(0, 0);
        localIp = getLocalIPAddress();

        // Don't wait to send broadcast
        broadcast_ping();
    }

    async void Update() {
        if (!waitingUDP) {
            waitingUDP = true; // TODO: this feels stupid. isn't there a better way ?
            var response = await udpClient.ReceiveAsync();
            waitingUDP = false;
            onReceive(response);
        }

        removeStaleSiblings();
        if (Time.time - lastBroadcastTime > pingInterval) broadcast_ping();
    }

    // "sorting station" for udp messages:
    // Calls the appropriate handler depending on the message type
    void onReceive(UdpReceiveResult response) {
        var message = UdpMessage.Parser.ParseFrom(response.Buffer);
        switch (message.MessageCase)
        {
            case UdpMessage.MessageOneofCase.PingMessage:
                onReceive(message.PingMessage);
                break;
            default:
                Debug.LogError("Received unexpected message format");
                break;
        }        
    }

    void onReceive(PingMessage ping) {
        if (ping.ToIp == "all") {
            sendPingResponse(ping);
        } else {
            double roundTripTime = Time.time - ping.SentTime;

            bool siblingsChanged = false;
            if (!siblings.ContainsKey(ping.ToIp)) {
                siblings[ping.ToIp] = new Sibling();
                siblingsChanged = true;
            }
            siblings[ping.ToIp].ip = ping.ToIp;
            siblings[ping.ToIp].lastPing = Time.time;
            siblings[ping.ToIp].latency = roundTripTime / 2.0f;

            if (siblingsChanged) onSiblingsChanged();
        }
    }

    void removeStaleSiblings() {
        List<string> staleSiblings = new List<string>();
        foreach (var keyValue in siblings) {
            if (Time.time - keyValue.Value.lastPing > siblingMaxStale) {
                staleSiblings.Add(keyValue.Key);
            }
        };

        if (staleSiblings.Count > 0) {
            staleSiblings.ForEach(key => { siblings.Remove(key); });
            onSiblingsChanged();
        }
    }

    void broadcast_ping() {
        lastBroadcastTime = Time.time;

        var data = new UdpMessage() {
            PingMessage = new PingMessage() {
                FromIp = localIp,
                ToIp = "all",
                SentTime = Time.time,
            }
        }.ToByteArray();
        udpClient.Send(data, data.Length, "255.255.255.255", port);
    }

    void sendPingResponse(PingMessage ping) {
        var data = new UdpMessage() {
            PingMessage = new PingMessage() {
                FromIp = ping.FromIp,
                ToIp = localIp,
                SentTime = ping.SentTime,
            }
        }.ToByteArray();
        udpClient.Send(data, data.Length, ping.FromIp, port);
    }

    public static string getLocalIPAddress() {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
        }
        return "0.0.0.0";
    }

    void onSiblingsChanged() {
        Debug.Log("siblings changed");
    }

    void OnGUI()
    {
        string infoString = "";
        infoString += String.Format("local IP:\n  {0}\n", localIp);
        infoString += "siblings (IP, last ping, latency):\n";
        foreach (var sibling in siblings.Values) {
            infoString += String.Format("  - {0}\t{1:0.00}s\t{2:0.00}ms\n", sibling.ip, Time.time - sibling.lastPing, sibling.latency*1000);
        }
        GUI.TextArea(new Rect(0,0,300,100), infoString);
    }
}