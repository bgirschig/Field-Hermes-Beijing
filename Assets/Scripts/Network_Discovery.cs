using System.Collections;
 using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

public class Network_Discovery : MonoBehaviour
{
    class SiblingInfo {
        public string ip;
        public float lastPing;
        public float latency;
    }

    struct PingMessage {
        public string ip;
        public float senderTime;

        public byte[] toBytes() {
            var stringData = String.Join(",", ip, senderTime);
            return Encoding.UTF8.GetBytes(stringData);
        }
        public static PingMessage fromBytes(byte[] buffer) {
            var text = Encoding.UTF8.GetString(buffer);
            var parts = text.Split(',');
            return new PingMessage() {
                ip = parts[0],
                senderTime = float.Parse(parts[1])
            };
        }
    }

    struct PingResponse {
        public string ip;
        // Keep track of the sender's time when he sent the ping, so that it can compute the Round trip time
        // upon receiving this response
        public float originalSenderTime;

        public byte[] toBytes() {
            var stringData = String.Join(",", ip, originalSenderTime);
            return Encoding.UTF8.GetBytes(stringData);
        }
    }

    public int port = 9876;
    public float siblingMaxStale = 5;
    public float pingInterval = 2;
    private UdpClient udpClient;
    private IPEndPoint from;

    private float lastBroadcastTime = 0;
    private string localIp = "0.0.0.0";
    // private List<SiblingInfo> siblings = new List<SiblingInfo>();
    private Dictionary<string, SiblingInfo> siblings = new Dictionary<string, SiblingInfo>();
    bool waitingUDP = false;

    void Start() {
        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

        from = new IPEndPoint(0, 0);
        localIp = getLocalIPAddress();

        broadcast();
    }

    async void Update() {
        if (!waitingUDP) {
            waitingUDP = true; // TODO: this feels stupid. isn't there a better way ?
            var response = await udpClient.ReceiveAsync();
            waitingUDP = false;

            onReceivedMessage(response);
        }

        removeStaleSiblings();

        if (Time.time - lastBroadcastTime > pingInterval) {
            broadcast();
        }
    }

    void onReceivedMessage(UdpReceiveResult response) {
        var responseData = PingMessage.fromBytes(response.Buffer);

        bool siblingsChanged = false;
        if (!siblings.ContainsKey(responseData.ip)) {
            siblings[responseData.ip] = new SiblingInfo();
            siblingsChanged = true;
        }
        siblings[responseData.ip].ip = responseData.ip;
        siblings[responseData.ip].lastPing = Time.time;
        siblings[responseData.ip].latency = responseData.senderTime - Time.time;

        if (siblingsChanged) onSiblingsChanged();
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

    void broadcast() {
        lastBroadcastTime = Time.time;
        var message = new PingMessage() { ip = localIp, senderTime = Time.time };
        var data = message.toBytes();
        udpClient.Send(data, data.Length, "255.255.255.255", port);
    }
    void sendPingResponse(PingMessage ping) {
        var message = new PingResponse() { ip = localIp, originalSenderTime = ping.senderTime };
        var data = message.toBytes();
        udpClient.Send(data, data.Length, ping.ip, port);
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
        infoString += "siblings (IP, last ping):\n";
        foreach (var sibling in siblings.Values) {
            infoString += String.Format("  - {0} {1}s\n", sibling.ip, Time.time - sibling.lastPing);
        }
        GUI.TextArea(new Rect(0,0,300,100), infoString);
    }
}