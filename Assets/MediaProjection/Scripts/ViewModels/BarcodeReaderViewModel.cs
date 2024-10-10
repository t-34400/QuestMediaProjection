#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MediaProjection.ViewModels
{
    class BarcodeReaderViewModel : MonoBehaviour
    {
        [SerializeField] private MediaProjectionViewModel mediaProjectionViewModel = default!;
        [SerializeField] private Models.BarcodeFormat possibleFormats = Models.BarcodeFormat.QR_CODE;
        [SerializeField] private bool cropRequired = false;
        [SerializeField] private RectInt cropRect = new RectInt();
        [SerializeField] private bool tryHarder = false;
        [SerializeField] private UnityEvent<Models.BarcodeReadingResult[]> barcodeRead = default!;

        private Services.IBarcodeReaderService? barcodeReaderService;

        private long latestResultTimestamp = long.MinValue;

        public void SetBarcodeReaderOptions(
            IEnumerable<Models.BarcodeFormat> possibleFormats,
            bool cropRequired = false,
            RectInt? cropRect = null,
            bool tryHarder = false)
        {
            barcodeReaderService?.Dispose();
            barcodeReaderService = mediaProjectionViewModel.ServiceContainer.GetBarcodeReaderService(
                    possibleFormats,
                    cropRequired,
                    cropRect ?? new RectInt(),
                    tryHarder
                );            
        }

        private void Awake()
        {
            SetBarcodeReaderOptions(ParseFormatFlags(possibleFormats), cropRequired, cropRect, tryHarder);
        }

        private void Update()
        {
            if (barcodeReaderService?.TryGetBarcodeReadingResult(out var result) ?? false)
            {
                var resultTimestamp = result.Timestamp;
                if (resultTimestamp != latestResultTimestamp)
                {
                    latestResultTimestamp = resultTimestamp;
                    barcodeRead.Invoke(new Models.BarcodeReadingResult[] { result });
                }
            }
        }

        private void OnDestroy()
        {
            barcodeReaderService?.Dispose();
            barcodeReaderService = null;
        }

        private static Models.BarcodeFormat[] ParseFormatFlags(Models.BarcodeFormat formats)
        {
             return Enum.GetValues(typeof(Models.BarcodeFormat))
                   .Cast<Models.BarcodeFormat>()
                   .Where(option => formats.HasFlag(option))
                   .ToArray();
        }
    }
}