#nullable enable

using UnityEngine;

namespace MediaProjection.Services
{
    class MediaProjectionService : IMediaProjectionService
    {
        private AndroidJavaObject? mediaProjectionManager;

        public MediaProjectionService(AndroidJavaObject? mediaProjectionManager)
        {
            this.mediaProjectionManager = mediaProjectionManager;
        }

        internal void SetMediaProjectionManager(AndroidJavaObject? mediaProjectionManager)
        {
            this.mediaProjectionManager = mediaProjectionManager;
        }

        public bool TryGetScreenCapture(bool textureRequired, out Texture2D texture)
        {
            texture = new Texture2D(1, 1);

            if (mediaProjectionManager == null)
            {
                return false;
            }

            var imageData = mediaProjectionManager.Call<byte[]>("getLatestImageIfAvailable", textureRequired);
            if (imageData != null && imageData.Length > 0)
            {
                texture.LoadImage(imageData);
                texture.Apply();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            mediaProjectionManager?.Dispose();
        }
    }
}
