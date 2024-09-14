#nullable enable

using System;
using UnityEngine;

namespace MediaProjection.Services
{
    interface IMediaProjectionService
    {
        bool TryGetScreenCapture(out Texture2D texture);
    }
}