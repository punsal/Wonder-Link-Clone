namespace Core.Camera.Provider.Interface
{
    /// <summary>
    /// Provides access to a camera instance, enabling interaction with camera
    /// functionality in a game or application context.
    /// </summary>
    public interface ICameraProvider
    {
        UnityEngine.Camera Camera { get; }
    }
}