using UnityEngine;

/// <summary>
/// Manages camera operations, specifically positioning and scaling the camera
/// to accommodate specific game elements such as a grid or board.
/// Ensures the camera is correctly positioned and sized for gameplay visibility.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    private Camera _camera;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    /// <summary>
    /// Centers the camera on a board based on the provided number of rows and columns.
    /// Adjusts the camera's position and orthographic size to fit the board within the viewport.
    /// </summary>
    /// <param name="rows">The number of rows in the board.</param>
    /// <param name="columns">The number of columns in the board.</param>
    public void CenterOnBoard(int rows, int columns)
    {
        var centerX = (columns - 1) / 2f;
        var centerY = -(rows - 1) / 2f;
        
        transform.position = new Vector3(centerX, centerY, transform.position.z);
        
        var verticalSize = rows / 2f;
        var horizontalSize = columns / (2f * _camera.aspect);
        
        // Use the larger size and add padding (1 unit)
        _camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) + 1f;
    }
}