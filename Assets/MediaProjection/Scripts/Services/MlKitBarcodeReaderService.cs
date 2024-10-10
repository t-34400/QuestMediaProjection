#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class MlKitBarcodeReaderService : IMultipleBarcodeReaderService
    {
        private readonly AndroidJavaObject barcodeReader;

        public MlKitBarcodeReaderService(
            AndroidJavaObject mediaProjectionManager,
            IEnumerable<Models.MlKitBarcodeFormat> possibleFormats)
        {
            barcodeReader = new AndroidJavaObject(
                "com.t34400.mediaprojectionlib.mlkit.MlKitBarcodeReader",
                mediaProjectionManager,
                string.Join(" ", possibleFormats)
            );
        }

        public bool TryGetBarcodeReadingResult(out Models.BarcodeReadingResults result)
        {
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
        }
    }
}