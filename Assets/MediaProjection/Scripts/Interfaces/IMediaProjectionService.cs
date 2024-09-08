#nullable enable

using System;
using UnityEngine;

namespace MediaProjection.Services
{
    interface IMediaProjectionService : IDisposable
    {
        bool TryGetScreenCapture(out Texture2D texture);
    }
}