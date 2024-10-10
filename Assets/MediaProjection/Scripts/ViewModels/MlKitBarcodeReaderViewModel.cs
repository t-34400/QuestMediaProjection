#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MediaProjection.ViewModels
{
    class MlKitBarcodeReaderViewModel : MonoBehaviour
    {
        [SerializeField] private MediaProjectionViewModel mediaProjectionViewModel = default!;
        [SerializeField] private Models.MlKitBarcodeFormat possibleFormats = Models.MlKitBarcodeFormat.FORMAT_QR_CODE;
        [SerializeField] private UnityEvent<Models.BarcodeReadingResult[]> barcodeRead = default!;

        private Services.IMultipleBarcodeReaderService? barcodeReaderService;

        private long latestResultTimestamp = long.MinValue;

        public void SetBarcodeReaderOptions(
            IEnumerable<Models.MlKitBarcodeFormat> possibleFormats)
        {
            barcodeReaderService?.Dispose();
            barcodeReaderService = mediaProjectionViewModel.ServiceContainer.GetMlKitBarcodeReaderService(possibleFormats);
        }

        private void Awake()
        {
            SetBarcodeReaderOptions(ParseFormatFlags(possibleFormats));
        }

        private void Update()
        {
            if ((barcodeReaderService?.TryGetBarcodeReadingResult(out var results) ?? false)
                && results.Results.Length > 0)
            {
                Debug.Log($"Hello: {results.Results.Length}");
                var resultTimestamp = results.Results[0].Timestamp;
                if (resultTimestamp != latestResultTimestamp)
                {
                    latestResultTimestamp = resultTimestamp;
                    barcodeRead.Invoke(results.Results);
                }
            }
        }

        private void OnDestroy()
        {
            barcodeReaderService?.Dispose();
            barcodeReaderService = null;
        }

        private static Models.MlKitBarcodeFormat[] ParseFormatFlags(Models.MlKitBarcodeFormat formats)
        {
             return Enum.GetValues(typeof(Models.MlKitBarcodeFormat))
                   .Cast<Models.MlKitBarcodeFormat>()
                   .Where(option => formats.HasFlag(option))
                   .ToArray();
        }
    }
}