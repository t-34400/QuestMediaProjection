#nullable enable

using System;
using System.Linq;
using UnityEngine;

namespace MediaProjection.Models
{
    [Serializable]
    public struct BarcodeReadingResult
    {
        [SerializeField] private string text;
        [SerializeField] private string format;
        [SerializeField] private int numBits;
        [SerializeField] private int[] rawBytes;
        [SerializeField] private ResultPointData[] resultPoints;
        [SerializeField] private long timestamp;

        public string Text => text;
        public string Format => format;
        public int NumBits => numBits;
        public byte[] RawBytes => rawBytes.Select(b => (byte)b).ToArray();
        public Vector2[] ResultPoints => resultPoints.Select(point => new Vector2(point.x, point.y)).ToArray();
        public long Timestamp => timestamp;
    }

    [Serializable]
    struct ResultPointData
    {
        public float x;
        public float y;
    }    
}