#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace MediaProjection.Services
{
    class BarcodeReaderService : IBarcodeReaderService
    {
        private readonly AndroidJavaObject barcodeReader;

        public BarcodeReaderService(
            AndroidJavaObject mediaProjectionManager,
            IEnumerable<Models.BarcodeFormat> possibleFormats,
            bool cropRequired,
            RectInt cropRange,
            bool tryHarder)
        {
            barcodeReader = new AndroidJavaObject(
                "com.t34400.mediaprojectionlib.zxing.BarcodeReader",
                mediaProjectionManager,
                string.Join(" ", possibleFormats),
                cropRequired,
                cropRange.xMin,
                cropRange.yMin,
                cropRange.width,
                cropRange.height,
                tryHarder
            );
        }

        public bool TryGetBarcodeReadingResult(out Models.BarcodeReadingResult result)
        {
            var resultJson = barcodeReader.Call<string>("getLatestResult");
            Debug.Log($"Barcode Reader Result: \n{resultJson}");

            if (!string.IsNullOrEmpty(resultJson))
            {
                result = JsonUtility.FromJson<Models.BarcodeReadingResult>(resultJson);
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