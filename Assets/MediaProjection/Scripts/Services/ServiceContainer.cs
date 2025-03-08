#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class ServiceContainer : MonoBehaviour
    {
        private AndroidJavaObject? mediaProjectionManager;
        private AndroidJavaObject? bitmapSaver;

        private MediaProjectionService? mediaProjectionService = null;

        private List<BarcodeReaderService> barcodeReaderServices = new();
        private List<MlKitBarcodeReaderService> mlKitBarcodeReaderServices = new();

        private string imageSaverFilenamePrefix = "";

        public IMediaProjectionService MediaProjectionService
        {
            get
            {
                mediaProjectionService ??= new MediaProjectionService(mediaProjectionManager);
                return mediaProjectionService;
            }
        }

        public IBarcodeReaderService GetBarcodeReaderService(
            IEnumerable<Models.BarcodeFormat> possibleFormats,
            bool cropRequired,
            RectInt cropRange,
            bool tryHarder)
        {
            var barcodeReaderService = new BarcodeReaderService(mediaProjectionManager, possibleFormats, cropRequired, cropRange, tryHarder);
            barcodeReaderServices.Add(barcodeReaderService);

            return barcodeReaderService;
        }

        public IMultipleBarcodeReaderService GetMlKitBarcodeReaderService(IEnumerable<Models.MlKitBarcodeFormat> possibleFormats)
        {
            var mlKitBarcodeReaderService = new MlKitBarcodeReaderService(mediaProjectionManager, possibleFormats);
            mlKitBarcodeReaderServices.Add(mlKitBarcodeReaderService);

            return mlKitBarcodeReaderService;
        }

        public void RequestImageSaver(string filenamePrefix)
        {
            imageSaverFilenamePrefix = filenamePrefix;

            if (bitmapSaver != null || string.IsNullOrEmpty(filenamePrefix) || mediaProjectionManager == null)
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
                            mediaProjectionManager,
                            filenamePrefix);
                }
            }
        }

        private void OnEnable()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    mediaProjectionManager = new AndroidJavaObject(
                            "com.t34400.mediaprojectionlib.core.MediaProjectionManager", 
                            activity);
                }
            }
            
            mediaProjectionService?.SetMediaProjectionManager(mediaProjectionManager);
            barcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(mediaProjectionManager));
            mlKitBarcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(mediaProjectionManager));

            RequestImageSaver(imageSaverFilenamePrefix);
        }

        private void OnDisable()
        {
            mediaProjectionService?.SetMediaProjectionManager(null);
            barcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(null));
            mlKitBarcodeReaderServices.ForEach(service => service.SetMediaProjectionManager(null));

            bitmapSaver?.Dispose();
            bitmapSaver = null;

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    mediaProjectionManager?.Call("stopMediaProjection", activity);
                }
            }
            mediaProjectionManager?.Dispose();
            mediaProjectionManager = null;
        }

        private void OnDestroy()
        {
            barcodeReaderServices.ForEach(service => service.Dispose());
            barcodeReaderServices.Clear();

            mlKitBarcodeReaderServices.ForEach(service => service.Dispose());
            mlKitBarcodeReaderServices.Clear();

            bitmapSaver?.Dispose();
            bitmapSaver = null;

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