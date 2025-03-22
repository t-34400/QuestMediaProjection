#nullable enable

using MediaProjection.WebRtc;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SignalingClient : MonoBehaviour
{
    [Header("WebRTC")]
    [SerializeField] private WebRtcMediaProjectionManager mediaProjectionManager = default!;
    [SerializeField] private List<string> iceServers = new() { "stun:stun.l.google.com:19302" };
    [SerializeField] private List<Constraint> constraints = new() { new Constraint { key = "OfferToReceiveVideo", value = "false" } };

    [Header("WebSocket")]
    [SerializeField] private string serverUrl = "ws://localhost:8080";

    private WebSocket? websocket = null;

    private PeerConnection? peerConnection = null;

    private float latestLogStateTime = 0f;

    private void Start()
    {
        ConnectToServer();
    }
    
    async void ConnectToServer()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
            {
                Debug.Log("Connected to signaling server");

                CreatePeerConnection();
            };
        websocket.OnError += (e) => Debug.LogError($"WebSocket Error: {e}");
        websocket.OnClose += (e) => Debug.Log("Disconnected from signaling server");
        websocket.OnMessage += (bytes) => OnWebSocketMessage(Encoding.UTF8.GetString(bytes));

        await websocket.Connect();
    }

    private void CreatePeerConnection()
    {
        peerConnection = mediaProjectionManager.CreatePeerConnection(
            iceServers, true, true
        );

        if (peerConnection == null)
        {
            Debug.LogError("Failed to create PeerConnection");
            enabled = false;
            return;
        }

        peerConnection.OnIceCandidate += async (candidate) =>
        {
            Debug.Log("SignalingClient: Ice candidate added");

            var candidateJson = JsonUtility.ToJson(candidate);

            var message = new SignalingMessage
            {
                type = "candidate",
                message = candidateJson
            };

            if (websocket != null)
            {
                await websocket.SendText(JsonUtility.ToJson(message));
            }
        };

        peerConnection.OnRenegotiationNeeded += () =>
        {
            Debug.Log("SignalingClient: On Renegotiation Needed");

            var offerObserver = new SdpObserver();
            offerObserver.OnCreateSuccess += async (desc) =>
            {
                Debug.Log("SignalingClient: Created local offer: " + desc.description);

                var localDescObserver = new SdpObserver();
                localDescObserver.OnSetSuccess += () => Debug.Log("SignalingClient: Set local description");
                localDescObserver.OnSetFailure += (error) => Debug.LogError("SignalingClient: Failed to set local description: " + error);
                localDescObserver.OnCreateFailure += (error) => Debug.LogError("SignalingClient: Failed to create local description: " + error);
                localDescObserver.OnCreateSuccess += (desc) => Debug.Log("SignalingClient: Created local description: " + desc.description);

                peerConnection.SetLocalDescription(localDescObserver, PeerConnection.SessionDescriptionType.OFFER, desc.description);
                
                var offerJson = JsonUtility.ToJson(desc);

                var message = new SignalingMessage
                {
                    type = "offer",
                    message = offerJson
                };

                if (websocket != null)
                {
                    await websocket.SendText(JsonUtility.ToJson(message));
                }
            };
            offerObserver.OnSetSuccess += () => Debug.Log("SignalingClient: Set local offer");
            offerObserver.OnCreateFailure += (error) => Debug.LogError("SignalingClient: Failed to create local offer: " + error);
            offerObserver.OnSetFailure += (error) => Debug.LogError("SignalingClient: Failed to set local offer: " + error);

            peerConnection.CreateOffer(offerObserver, constraints.ToDictionary(c => c.key, c => c.value));
        };
    }

    private void OnWebSocketMessage(string json)
    {
        Debug.Log("Received message: " + json);
        var message = JsonUtility.FromJson<SignalingMessage>(json);

        switch (message.type)
        {
            case "answer":
                {
                    var desc = JsonUtility.FromJson<SessionDescription>(message.message);
                    peerConnection?.SetRemoteDescription(new SdpObserver(), desc.type, desc.description);
                    break;
                }
            case "candidate":
                {
                    var candidate = JsonUtility.FromJson<JsIceCandidateData>(message.message);
                    peerConnection?.AddIceCandidate(candidate.sdpMid, candidate.sdpMLineIndex, candidate.candidate);
                    break;
                }
        }
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();

        if (Time.time - latestLogStateTime > 1f)
        {
            latestLogStateTime = Time.time;
            var iceConnectionState = peerConnection?.GetIceConnectionState();
            var iceGatheringState = peerConnection?.GetIceGatheringState();
            var signalingState = peerConnection?.GetSignalingState();
            var connectionState = peerConnection?.GetConnectionState();

            Debug.Log($"IceConnectionState: {iceConnectionState}, IceGatheringState: {iceGatheringState}, SignalingState: {signalingState}, ConnectionState: {connectionState}");

            var localDescription = peerConnection?.GetLocalDescription();
            Debug.Log("LocalDescription: " + localDescription);
        }
    }

    private async void OnDestroy()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }

        if (mediaProjectionManager != null && peerConnection != null)
        {
            mediaProjectionManager.RemovePeerConnection(peerConnection);
            peerConnection = null;
        }
    }

    [Serializable]
    struct Constraint
    {
        public string key;
        public string value;
    }

    [Serializable]
    struct SignalingMessage
    {
        public string type;
        public string message;
    }

    [Serializable]
    struct JsIceCandidateData
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex;
        public string usernameFragment;
    }
}