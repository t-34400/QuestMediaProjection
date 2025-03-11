#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class MlKitBarcodeReaderService : IMultipleBarcodeReaderService
    {
        private AndroidJavaObject? barcodeReader;

        private readonly IEnumerable<Models.MlKitBarcodeFormat> possibleFormats;

        public MlKitBarcodeReaderService(
            AndroidJavaObject? imageProcessManager,
            IEnumerable<Models.MlKitBarcodeFormat> possibleFormats)
        {
            this.possibleFormats = possibleFormats;
            SetMediaProjectionManager(imageProcessManager);
        }

        internal void SetMediaProjectionManager(AndroidJavaObject? mediaProjectionManager)
        {
            Dispose();

            if (mediaProjectionManager == null)
            {
                return;
            }

            barcodeReader = new AndroidJavaObject(
                "com.t34400.mediaprojectionlib.mlkit.MlKitBarcodeReader",
                mediaProjectionManager,
                string.Join(" ", possibleFormats)
            );
        }

        public bool TryGetBarcodeReadingResult(out Models.BarcodeReadingResults result)
        {
            if (barcodeReader == null)
            {
                result = default;
                return false;
            }

            var resultJson = barcodeReader.Call<string>("getLatestResult");
            Debug.Log($"Barcode Reader Result: \n{resultJson}");

            if (!string.IsNullOrEmpty(resultJson))
            {
                result = JsonUtility.FromJson<Models.BarcodeReadingResults>(resultJson);
                return true;
            }

            result = default;
            return false;
        }

        public void Dispose()
        {
            barcodeReader?.Call("close");
            barcodeReader?.Dispose();
            barcodeReader = null;
        }
    }
}