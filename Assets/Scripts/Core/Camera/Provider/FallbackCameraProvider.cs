using Core.Camera.Provider.Interface;

namespace Core.Camera.Provider
{
    /// <summary>
    /// Provides a fallback implementation of the `ICameraProvider` interface.
    /// It is used to supply a default camera instance when no specific camera provider
    /// is available.
    /// </summary>
    public class FallbackCameraProvider : ICameraProvider
    {
        public UnityEngine.Camera Camera { get; private set; }

        public FallbackCameraProvider(UnityEngine.Camera camera)
        {
            Camera = camera;
        }
    }
}