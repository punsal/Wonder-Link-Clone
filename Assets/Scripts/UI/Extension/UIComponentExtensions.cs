using UnityEngine;

namespace UI.Extension
{
    /// <summary>
    /// Provides extension methods for Unity's UI components, enhancing their functionality
    /// with utility operations to simplify common tasks.
    /// </summary>
    public static class UIComponentExtensions
    {
        public static void Show(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup is null");
                return;
            }
            
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public static void Hide(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup is null");
                return;
            }
            
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}