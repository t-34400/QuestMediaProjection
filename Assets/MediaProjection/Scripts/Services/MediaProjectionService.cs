#nullable enable

using UnityEngine;

namespace MediaProjection.Services
{
    class MediaProjectionService : IMediaProjectionService
    {
        private AndroidJavaObject? imageProcessManager;

        public MediaProjectionService(AndroidJavaObject? imageProcessManager)
        {
            this.imageProcessManager = imageProcessManager;
        }

        internal void SetMediaProjectionManager(AndroidJavaObject? mediaProjectionManager)
        {
            this.imageProcessManager = mediaProjectionManager;
        }

        public bool TryGetScreenCapture(bool textureRequired, out Texture2D texture)
        {
            texture = new Texture2D(1, 1);

            if (imageProcessManager == null)
            {
                return false;
            }

            var imageData = imageProcessManager.Call<byte[]>("getLatestImageIfAvailable", textureRequired);
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
            imageProcessManager?.Dispose();
        }
    }
}
