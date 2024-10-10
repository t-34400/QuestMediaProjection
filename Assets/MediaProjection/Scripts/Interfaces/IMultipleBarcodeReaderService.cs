#nullable enable

using System;

namespace MediaProjection.Services
{
    interface IMultipleBarcodeReaderService : IDisposable
    {
        bool TryGetBarcodeReadingResult(out Models.BarcodeReadingResults result);
    }
}