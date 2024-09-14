#nullable enable

using System;

namespace MediaProjection.Models
{
    [Flags]
    enum BarcodeFormat
    {
        AZTEC = 1 << 0,
        CODABAR = 1 << 1,
        CODE_128 = 1 << 2,
        CODE_39 = 1 << 3,
        CODE_93 = 1 << 4,
        DATA_MATRIX = 1 << 5,
        EAN_13 = 1 << 6,
        EAN_8 = 1 << 7,
        ITF = 1 << 8,
        MAXICODE = 1 << 9,
        PDF_417 = 1 << 10,
        QR_CODE = 1 << 11,
        RSS_14 = 1 << 12,
        RSS_EXPANDED = 1 << 13,
        UPC_A = 1 << 14,
        UPC_E = 1 << 15,
        UPC_EAN_EXTENSION = 1 << 16,
    }
}