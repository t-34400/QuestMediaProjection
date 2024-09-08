#nullable enable

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace MediaProjection.ViewModels
{
    class MediaProjectionViewModel : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Texture2D> screenUpdated = default!;
        [SerializeField] private float minUpdateInterval = 0.05f;

        private Services.IMediaProjectionService? mediaProjectionService = null;

        private Texture2D? currentTexture;
        private float latestUpdateTime = float.NegativeInfinity;

        public Texture2D CurrentTexture
        {
            get
            {
                currentTexture ??= new Texture2D(1, 1);
                return currentTexture;
            }
            set
            {
                currentTexture = value;
                screenUpdated.Invoke(currentTexture);
            }
        }

        private void Awake()
        {
            mediaProjectionService = Services.ServiceContainer.MediaProjectionService;
        }

        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime - latestUpdateTime > minUpdateInterval)
            {
                latestUpdateTime = currentTime;
                
                if (mediaProjectionService != null
                    && mediaProjectionService.TryGetScreenCapture(out var texture))
                {
                    CurrentTexture = texture;
                }
            }
        }

        private void OnDestroy()
        {
            mediaProjectionService?.Dispose();
            mediaProjectionService = null;
        }
    }
}