#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MediaProjection.WebRtc
{
    public class PeerConnection : IDisposable
    {
        private AndroidJavaObject? peerConnection;
        private AndroidJavaObject? peerConnectionObserver;
        private List<SdpObserver> sdpObservers = new List<SdpObserver>();

        public PeerConnection(AndroidJavaObject peerConnectionObject, AndroidJavaObject peerConnectionObserverObject)
        {
            peerConnection = peerConnectionObject;
            peerConnectionObserver = peerConnectionObserverObject;
        }

        internal AndroidJavaObject? PeerConnectionObject => peerConnection;

        public void Dispose()
        {
            sdpObservers.ForEach(observer => observer.Dispose());
            sdpObservers.Clear();

            peerConnectionObserver?.Dispose();
            peerConnectionObserver = null;

            peerConnection?.Call("dispose");
            peerConnection?.Dispose();
            peerConnection = null;
        }

# region SDP communication

        public void CreateOffer(SdpObserver observer, Dictionary<string, string> constraints)
        {
            var observerObject = observer.SdpObserverObject;

            if (peerConnection == null || observer.IsDisposed || observerObject == null)
            {
                Debug.LogError("PeerConnection or SdpObserver is null or disposed.");
                return;
            }

            sdpObservers.Add(observer);

            var constraintsJson = ConvertDictionaryToJson(constraints);
            peerConnection?.Call("createOffer", observerObject, constraintsJson);
        }

        public void CreateAnswer(SdpObserver observer, Dictionary<string, string> constraints)
        {
            var observerObject = observer.SdpObserverObject;

            if (peerConnection == null || observer.IsDisposed || observerObject == null)
            {
                Debug.LogError("PeerConnection or SdpObserver is null or disposed.");
                return;
            }

            sdpObservers.Add(observer);

            var constraintsJson = ConvertDictionaryToJson(constraints);
            peerConnection?.Call("createAnswer", observerObject, constraintsJson);
        }

        public string GetLocalDescription()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return string.Empty;
            }

            return peerConnection.Call<string>("getLocalDescription");
        }

        public void GetRemoteDescription(SdpObserver observer)
        {
            var observerObject = observer.SdpObserverObject;

            if (peerConnection == null || observer.IsDisposed || observerObject == null)
            {
                Debug.LogError("PeerConnection or SdpObserver is null or disposed.");
                return;
            }

            sdpObservers.Add(observer);

            peerConnection?.Call("getRemoteDescription", observerObject);
        }

        public void SetLocalDescription(SdpObserver observer)
        {
            var observerObject = observer.SdpObserverObject;

            if (peerConnection == null || observer.IsDisposed || observerObject == null)
            {
                Debug.LogError("PeerConnection or SdpObserver is null or disposed.");
                return;
            }

            sdpObservers.Add(observer);

            peerConnection?.Call("setLocalDescription", observerObject);
        }

        public void SetRemoteDescription(SdpObserver observer, string type, string description)
        {
            var observerObject = observer.SdpObserverObject;

            if (peerConnection == null || observer.IsDisposed || observerObject == null)
            {
                Debug.LogError("PeerConnection or SdpObserver is null or disposed.");
                return;
            }

            sdpObservers.Add(observer);

            peerConnection?.Call("setRemoteDescription", observerObject, type, description);
        }

#endregion

# region ICE communication

        public bool AddIceCandidate(string sdpMid, int sdpMLineIndex, string sdp)
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return false;
            }

            return peerConnection.Call<bool>("addIceCandidate", sdpMid, sdpMLineIndex, sdp);
        }

        public void RestartIce()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return;
            }

            peerConnection.Call("restartIce");
        }

#endregion

# region State
        public PeerConnectionState GetConnectionState()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return PeerConnectionState.FAILED;
            }

            var state = peerConnection.Call<string>("getConnectionState");
            if (Enum.TryParse(state, out PeerConnectionState connectionState))
            {
                return connectionState;
            }

            return PeerConnectionState.FAILED;
        }

        public IceConnectionState GetIceConnectionState()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return IceConnectionState.FAILED;
            }

            var state = peerConnection.Call<string>("getIceConnectionState");
            if (Enum.TryParse(state, out IceConnectionState iceConnectionState))
            {
                return iceConnectionState;
            }

            return IceConnectionState.FAILED;
        }

        public IceGatheringState GetIceGatheringState()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return IceGatheringState.NEW;
            }

            var state = peerConnection.Call<string>("getIceGatheringState");
            if (Enum.TryParse(state, out IceGatheringState iceGatheringState))
            {
                return iceGatheringState;
            }

            return IceGatheringState.NEW;
        }

        public SignalingState GetSignalingState()
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return SignalingState.CLOSED;
            }

            var state = peerConnection.Call<string>("getSignalingState");
            if (Enum.TryParse(state, out SignalingState signalingState))
            {
                return signalingState;
            }

            return SignalingState.CLOSED;
        }

        public void SetAudioPlayout(bool enable)
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return;
            }

            peerConnection.Call("setAudioPlayout", enable);
        }

        public void SetAudioRecording(bool enable)
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return;
            }

            peerConnection.Call("setAudioRecording", enable);
        }

        public void SetBitrate(int min, int current, int max)
        {
            if (peerConnection == null)
            {
                Debug.LogError("PeerConnection is null.");
                return;
            }

            peerConnection.Call("setBitrate", min, current, max);
        }

# endregion

# region Observer Events
        public event Action<SignalingState>? OnSignalingChange;
        public event Action<IceConnectionState>? OnIceConnectionChange;
        public event Action<bool>? OnIceConnectionReceivingChange;
        public event Action<IceGatheringState>? OnIceGatheringChange;
        public event Action<IceCandidateData>? OnIceCandidate;
        public event Action? OnNegotiationNeeded;
# endregion

        internal void ObserveState()
        {
            if (peerConnectionObserver == null)
            {
                Debug.LogError("PeerConnectionObserver is null.");
            }
            else
            {
                var eventLogJson = peerConnectionObserver.Call<string>("getEventLogJson");
                if (string.IsNullOrEmpty(eventLogJson))
                {
                    Debug.LogError("Event log is empty");
                    return;
                }

                var eventLog = JsonUtility.FromJson<EventLog>(eventLogJson);
                foreach (var entry in eventLog.dataList)
                {
                    switch (entry.key)
                    {
                        case "onSignalingChange":
                            {
                                if (Enum.TryParse(entry.value, out SignalingState signalingState))
                                {
                                    OnSignalingChange?.Invoke(signalingState);
                                }
                                break;
                            }
                        case "onIceConnectionChange":
                            {
                                if (Enum.TryParse(entry.value, out IceConnectionState iceConnectionState))
                                {
                                    OnIceConnectionChange?.Invoke(iceConnectionState);
                                }
                                break;
                            }
                        case "onIceConnectionReceivingChange":
                            {
                                if (bool.TryParse(entry.value, out bool receiving))
                                {
                                    OnIceConnectionReceivingChange?.Invoke(receiving);
                                }
                                break;
                            }
                        case "onIceGatheringChange":
                            {
                                if (Enum.TryParse(entry.value, out IceGatheringState iceGatheringState))
                                {
                                    OnIceGatheringChange?.Invoke(iceGatheringState);
                                }
                                break;
                            }
                        case "onIceCandidate":
                            {
                                var iceCandidateData = JsonUtility.FromJson<IceCandidateData>(entry.value);
                                OnIceCandidate?.Invoke(iceCandidateData);
                                break;
                            }
                        case "onNegotiationNeeded":
                            {
                                OnNegotiationNeeded?.Invoke();
                                break;
                            }
                    }                    
                }
            }


            sdpObservers.ForEach(observer => observer.ObserveState());
        }

        static string ConvertDictionaryToJson(Dictionary<string, string> dict)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");

            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",\n");
                sb.Append($"  \"{kvp.Key}\": \"{kvp.Value}\"");
                first = false;
            }

            sb.Append("\n}");
            return sb.ToString();
        }

        public enum PeerConnectionState
        {
            NEW,
            CONNECTING,
            CONNECTED,
            DISCONNECTED,
            FAILED,
            CLOSED
        }

        public enum IceConnectionState
        {
            NEW,
            CHECKING,
            CONNECTED,
            COMPLETED,
            FAILED,
            DISCONNECTED,
            CLOSED
        }

        public enum IceGatheringState
        {
            NEW,
            GATHERING,
            COMPLETE
        }

        public enum SignalingState
        {
            STABLE,
            HAVE_LOCAL_OFFER,
            HAVE_LOCAL_PRANSWER,
            HAVE_REMOTE_OFFER,
            HAVE_REMOTE_PRANSWER,
            CLOSED
        }
    }

    public class SdpObserver : IDisposable
    {
        private AndroidJavaObject? sdpObserverObject;

        public SdpObserver()
        {
            sdpObserverObject = new AndroidJavaObject("com.t34400.mediaprojectionlib.webrtc.PeerConnectionSdpObserver");
        }

        internal bool IsDisposed { get; private set; } = false;
        internal AndroidJavaObject? SdpObserverObject => sdpObserverObject;

        public event Action<SessionDescription>? OnCreateSuccess;
        public event Action? OnSetSuccess;
        public event Action<string>? OnCreateFailure;
        public event Action<string>? OnSetFailure;

        public void Dispose()
        {
            sdpObserverObject?.Call("dispose");
            sdpObserverObject?.Dispose();
            sdpObserverObject = null;

            IsDisposed = true;
        }

        internal void ObserveState()
        {
            var eventLogJson = sdpObserverObject?.Call<string>("getEventLogJson");
            if (string.IsNullOrEmpty(eventLogJson))
            {
                Debug.LogError("Event log is empty");
                return;
            }

            var eventLog = JsonUtility.FromJson<EventLog>(eventLogJson);
            foreach (var entry in eventLog.dataList)
            {
                switch (entry.key)
                {
                    case "onCreateSuccess":
                        {
                            Debug.Log("onCreateSuccess: " + entry.value);

                            var sessionDescription = JsonUtility.FromJson<SessionDescription>(entry.value);
                            OnCreateSuccess?.Invoke(sessionDescription);

                            break;
                        }
                    case "onSetSuccess":
                        {
                            Debug.Log("onSetSuccess");
                            OnSetSuccess?.Invoke();
                            break;
                        }
                    case "onCreateFailure":
                        {
                            Debug.Log("onCreateFailure:" + entry.value);
                            OnCreateFailure?.Invoke(entry.value);
                            break;                            
                        }
                    case "onSetFailure":
                        {
                            Debug.Log("onSetFailure:" + entry.value);
                            OnSetFailure?.Invoke(entry.value);
                            break;
                        }
                }
            }
        }
    }

    [Serializable]
    public struct IceCandidateData
    {
        public string adapterType;
        public string sdp;
        public string sdpMid;
        public string sdpMLineIndex;
        public string serverUrl;
    }

    [Serializable]
    public struct SessionDescription
    {
        public string type;
        public string description;
    }
}