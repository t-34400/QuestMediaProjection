#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.WebRtc
{
    public class WebRtcMediaProjectionManager : MonoBehaviour
    {
        [SerializeField] private Services.ServiceContainer serviceContainer = default!;
        [SerializeField] private float stateUpdateInterval = 0.1f;

        private float latestStateUpdateTime = float.MinValue;

        private readonly List<PeerConnection> peerConnections = new ();

        public PeerConnection? CreatePeerConnection(
            IEnumerable<string> iceServers,
            bool isVideoTrackRequested,
            bool isAudioTrackRequested)
        {
            var mediaProjectionManager = serviceContainer.MediaProjectionManager;
            if (mediaProjectionManager == null)
            {
                Debug.LogError("MediaProjectionManager is not available");
                return null;
            }

            var iceServerString = string.Join(" ", iceServers);
            var peerConnectionObserverObject = new AndroidJavaObject(
                "com.t34400.mediaprojectionlib.webrtc.PeerConnectionObserver");

            var peerConnectionObject = mediaProjectionManager.Call<AndroidJavaObject>(
                "createPeerConnection",
                peerConnectionObserverObject,
                iceServerString,
                isVideoTrackRequested,
                isAudioTrackRequested);

            var peerConnection = new PeerConnection(peerConnectionObject, peerConnectionObserverObject);
            peerConnections.Add(peerConnection);

            return peerConnection;
        }

        public void RemovePeerConnection(PeerConnection peerConnection)
        {
            var mediaProjectionManager = serviceContainer.MediaProjectionManager;
            if (mediaProjectionManager != null)
            {
                Debug.LogError("MediaProjectionManager is not available");
                mediaProjectionManager.Call("removePeerConnection", peerConnection.PeerConnectionObject);
            }

            peerConnections.Remove(peerConnection);
            peerConnection.Dispose();
        }

        private void Awake()
        {
            serviceContainer.MediaProjectionManagerChanged += OnMediaProjectionManagerChanged;
        }

        private void OnMediaProjectionManagerChanged(AndroidJavaObject? mediaProjectionManager)
        {
            peerConnections.ForEach(pc => pc.Dispose());
            peerConnections.Clear();
        }

        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime - latestStateUpdateTime < stateUpdateInterval)
            {
                return;
            }

            latestStateUpdateTime = currentTime;
            peerConnections.ForEach(pc => pc.ObserveState());
        }

        private void OnDestroy()
        {
            peerConnections.Clear();
        }
    }
}