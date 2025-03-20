#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class ServiceContainer : MonoBehaviour
    {
        [SerializeField] private bool enableImageProcessing = true;

        private AndroidJavaObject? mediaProjectionCallback;
        private AndroidJavaObject? mediaProjectionManager;
        private AndroidJavaObject? imageProcessManager;
        private AndroidJavaObject? bitmapSaver;

        private MediaProjectionService? mediaProjectionService = null;

        private List<BarcodeReaderService> barcodeReaderServices = new();
        private List<MlKitBarcodeReaderService> mlKitBarcodeReaderServices = new();

        private string imageSaverFilenamePrefix = "";

        internal Func<AndroidJavaObject, AndroidJavaObject, AndroidJavaObject> GetMediaProjectionManager = (activity, callback) => 
            new AndroidJavaObject(
                "com.t34400.mediaprojectionlib.core.MediaProjectionManager", 
                activity, callback);

        public IMediaProjectionService MediaProjectionService
        {
            get
            {
                mediaProjectionService ??= new MediaProjectionService(imageProcessManager);
                return mediaProjectionService;
            }
        }

        internal AndroidJavaObject? MediaProjectionManager => mediaProjectionManager;
        internal event Action<AndroidJavaObject?>? MediaProjectionManagerChanged;

        public IBarcodeReaderService GetBarcodeReaderService(
            IEnumerable<Models.BarcodeFormat> possibleFormats,
            bool cropRequired,
            RectInt cropRange,
            bool tryHarder)
        {
            var barcodeReaderService = new BarcodeReaderService(imageProcessManager, possibleFormats, cropRequired, cropRange, tryHarder);
            barcodeReaderServices.Add(barcodeReaderService);

            return barcodeReaderService;
        }

        public IMultipleBarcodeReaderService GetMlKitBarcodeReaderService(IEnumerable<Models.MlKitBarcodeFormat> possibleFormats)
        {
            var mlKitBarcodeReaderService = new MlKitBarcodeReaderService(imageProcessManager, possibleFormats);
            mlKitBarcodeReaderServices.Add(mlKitBarcodeReaderService);

            return mlKitBarcodeReaderService;
        }

        public void RequestImageSaver(string filenamePrefix)
        {
            imageSaverFilenamePrefix = filenamePrefix;

            if (bitmapSaver != null || string.IsNullOrEmpty(filenamePrefix) || imageProcessManager == null)
            {
                return;
            }

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    bitmapSaver = new AndroidJavaObject(
                            "com.t34400.mediaprojectionlib.io.BitmapSaver", 
                            activity,
                            imageProcessManager,
                            filenamePrefix);
                }
            }
        }

        private void OnEnable()
        {
            mediaProjectionCallback = new AndroidJavaObject("com.t34400.mediaprojectionlib.core.MediaProjectionCallback");

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    mediaProjectionManager = GetMediaProjectionManager(activity, mediaProjectionCallback);

                    if (enableImageProcessing)
                    {
                        imageProcessManager = new AndroidJavaObject(
                                "com.t34400.mediaprojectionlib.core.ScreenImageProcessManager", 
                                mediaProjectionManager);
                    }
                }
            }
            
            mediaProjectionService?.SetMediaProjectionManager(imageProcessManager);
            barcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(imageProcessManager));
            mlKitBarcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(imageProcessManager));

            RequestImageSaver(imageSaverFilenamePrefix);

            MediaProjectionManagerChanged?.Invoke(mediaProjectionManager);
        }

        private void OnDisable()
        {
            mediaProjectionService?.SetMediaProjectionManager(null);
            barcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(null));
            mlKitBarcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(null));

            bitmapSaver?.Dispose();
            bitmapSaver = null;

            imageProcessManager?.Dispose();
            imageProcessManager = null;

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    mediaProjectionManager?.Call("stopMediaProjection", activity);
                }
            }
            mediaProjectionManager?.Dispose();
            mediaProjectionManager = null;

            mediaProjectionCallback?.Dispose();
            mediaProjectionCallback = null;

            MediaProjectionManagerChanged?.Invoke(null);
        }

        private void OnDestroy()
        {
            barcodeReaderServices.ForEach(service => service.Dispose());
            barcodeReaderServices.Clear();

            mlKitBarcodeReaderServices.ForEach(service => service.Dispose());
            mlKitBarcodeReaderServices.Clear();

            bitmapSaver?.Dispose();
            bitmapSaver = null;

            imageProcessManager?.Dispose();
            imageProcessManager = null;

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    mediaProjectionManager?.Call("stopMediaProjection", activity);
                }
            }
            mediaProjectionService?.Dispose();
            mediaProjectionService = null;
        }
    }
}