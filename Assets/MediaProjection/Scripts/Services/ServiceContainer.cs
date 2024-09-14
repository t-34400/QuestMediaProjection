#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class ServiceContainer : MonoBehaviour
    {
        private AndroidJavaObject? mediaProjectionManager;

        private MediaProjectionService? mediaProjectionService = null;

        private AndroidJavaObject MediaProjectionManager
        {
            get
            {
                if (mediaProjectionManager != null)
                {
                    return mediaProjectionManager;
                }

                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        mediaProjectionManager = new AndroidJavaObject(
                                "com.t34400.mediaprojectionlib.core.MediaProjectionManager", 
                                activity);
                        return mediaProjectionManager;
                    }                
                }
            }
        }

        public IMediaProjectionService MediaProjectionService
        {
            get
            {
                mediaProjectionService ??= new MediaProjectionService(MediaProjectionManager);
                return mediaProjectionService;
            }
        }

        public IBarcodeReaderService GetBarcodeReaderService(
            IEnumerable<Models.BarcodeFormat> possibleFormats,
            bool cropRequired,
            RectInt cropRange,
            bool tryHarder)
        {
            return new BarcodeReaderService(MediaProjectionManager, possibleFormats, cropRequired, cropRange, tryHarder);
        }

        private void OnDestroy()
        {
            mediaProjectionService?.Dispose();
            mediaProjectionService = null;
        }
    }
}