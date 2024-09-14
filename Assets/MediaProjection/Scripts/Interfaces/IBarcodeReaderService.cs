#nullable enable

using System;

namespace MediaProjection.Services
{
    interface IBarcodeReaderService : IDisposable
    {
        bool TryGetBarcodeReadingResult(out Models.BarcodeReadingResult result);
    }
}