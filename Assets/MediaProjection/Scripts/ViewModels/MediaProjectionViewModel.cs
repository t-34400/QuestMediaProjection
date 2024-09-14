#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace MediaProjection.ViewModels
{
    class MediaProjectionViewModel : MonoBehaviour
    {
        [SerializeField] private Services.ServiceContainer serviceContainer = default!;
        [SerializeField] private float minUpdateInterval = 0.05f;
        [Header("Screen Texture")]
        [Tooltip("Retrieves the texture captured from the screen. Uncheck this to disable texture processing if it is not needed, which will help improve performance.")]
        [SerializeField] private bool textureRequired = false;
        [SerializeField] private UnityEvent<Texture2D> textureUpdated = default!;

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
                textureUpdated.Invoke(currentTexture);
            }
        }

        public Services.ServiceContainer ServiceContainer => serviceContainer;

        private void Awake()
        {
            mediaProjectionService = serviceContainer.MediaProjectionService;
        }

        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime - latestUpdateTime > minUpdateInterval)
            {
                latestUpdateTime = currentTime;
                
                if (mediaProjectionService != null
                    && mediaProjectionService.TryGetScreenCapture(textureRequired, out var texture))
                {
                    CurrentTexture = texture;
                }
            }
        }
    }
}