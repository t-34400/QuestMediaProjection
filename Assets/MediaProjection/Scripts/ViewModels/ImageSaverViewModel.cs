#nullable enable

using UnityEngine;

namespace MediaProjection.ViewModels
{
    class ImageSaverViewModel : MonoBehaviour
    {
        [SerializeField] private MediaProjectionViewModel mediaProjectionViewModel = default!;
        [SerializeField] private string filenamePrefix = "capture_";

        private void Awake()
        {
            mediaProjectionViewModel.ServiceContainer.RequestImageSaver(filenamePrefix);
        }
    }
}