#nullable enable

using UnityEngine;

namespace MediaProjection.Services
{
    class MediaProjectionService : IMediaProjectionService
    {
        private readonly AndroidJavaObject mediaProjectionManager;

        public MediaProjectionService(AndroidJavaObject mediaProjectionManager)
        {
            this.mediaProjectionManager = mediaProjectionManager;
        }

        public bool TryGetScreenCapture(out Texture2D texture)
        {
            texture = new Texture2D(1, 1);

            var imageData = mediaProjectionManager.Call<byte[]>("getLatestImageIfAvailable");
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
            mediaProjectionManager.Dispose();
        }
    }
}
