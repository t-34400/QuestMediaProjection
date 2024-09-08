#nullable enable

namespace MediaProjection.Services
{
    static class ServiceContainer
    {
        private static MediaProjectionService? mediaProjectionService = null;

        public static IMediaProjectionService MediaProjectionService
        {
            get
            {
                mediaProjectionService ??= new MediaProjectionService();
                return mediaProjectionService;
            }
        }
    }
}