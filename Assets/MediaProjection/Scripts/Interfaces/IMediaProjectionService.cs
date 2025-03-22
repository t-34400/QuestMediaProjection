#nullable enable

using UnityEngine;

namespace MediaProjection.Services
{
    public interface IMediaProjectionService
    {
        bool TryGetScreenCapture(bool textureRequired, out Texture2D texture);
    }
}