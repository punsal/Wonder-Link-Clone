using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.Board.Abstract;
using Core.Camera;
using Core.Camera.Abstract;
using Core.Camera.Provider;
using Core.Camera.Provider.Abstract;
using Core.Link;
using Core.Link.Abstract;
using Core.Link.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private UnityCameraProviderBase gameCameraProvider;
    
    [Header("Board")]
    [SerializeField, Range(4, 12)] private int rowCount;
    [SerializeField, Range(4, 12)] private int columnCount;
    [SerializeField] private Tile tilePrefab;
    
    [Header("Chip")]
    [SerializeField] private LinkableBase[] chipPrefabs;
    
    [Header("Linking")]
    [SerializeField] private UnityCameraProviderBase linkCameraProvider;
    [SerializeField] private LayerMask linkLayerMask;

    private CameraSystemBase _cameraSystem;
    private BoardSystemBase _boardSystem;
    private LinkSystemBase _linkSystem;
    private List<LinkableBase> _chips;
    
    private void Awake()
    {
        CreateGameCameraSystem();
        CreateBoardSystem();
        CreateLinkSystem();
        _chips = new List<LinkableBase>();
    }
    
    private void OnEnable()
    {
        _boardSystem.Initialize();
        
        _linkSystem.OnInputCompleted += HandleLinkComplete;
    }

    private void Start()
    {
        CreateChips();
        _cameraSystem.CenterOnBoard(rowCount, columnCount);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            _linkSystem.StartDrag();
        }
        else if (Input.GetMouseButton(0) && _linkSystem.IsDragging)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            _linkSystem.UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _linkSystem.IsDragging)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            _linkSystem.EndDrag();
        }
    }

    private void OnDisable()
    {
        _linkSystem.OnInputCompleted -= HandleLinkComplete;
        
        DestroyChips();
        _boardSystem.Dispose();
    }

    private void CreateGameCameraSystem()
    {
        if (gameCameraProvider == null)
        {
            Debug.LogWarning("Game camera provider is null");
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found");
                _cameraSystem = new CameraSystem(null);
            }
            else
            {
                Debug.LogWarning("Using main camera");
                _cameraSystem = new CameraSystem(new FallbackCameraProvider(Camera.main));
            }
        }
        else
        {
            _cameraSystem = new CameraSystem(gameCameraProvider);
        }
    }

    private void CreateBoardSystem()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is null");
        }
        _boardSystem = new BoardSystem(rowCount, columnCount, tilePrefab);
    }

    private void CreateLinkSystem()
    {
        if (linkCameraProvider == null)
        {
            Debug.LogWarning("Link camera provider is null");
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found");
                _linkSystem = new LinkSystem(null, linkLayerMask);
            }
            else
            {
                Debug.LogWarning("Using main camera");
                _linkSystem = new LinkSystem(new FallbackCameraProvider(Camera.main), linkLayerMask);
            }
        }
        _linkSystem = new LinkSystem(linkCameraProvider, linkLayerMask);
    }

    private void CreateChips()
    {
        var chipPrefabsCount = chipPrefabs.Length;
        var boardSize = rowCount * columnCount;
        for (var i = 0; i < boardSize; i++)
        {
            if (!_boardSystem.TryGetEmptyTile(out var emptyTile))
            {
                Debug.LogWarning("No empty tiles");
                break;
            }
            var randomIndex = Random.Range(0, chipPrefabsCount);
            var chipPrefab = chipPrefabs[randomIndex];
            var chip = Instantiate(chipPrefab, Vector3.zero, Quaternion.identity);
            chip.Occupy(emptyTile);
            _boardSystem.AddOccupant(chip);
            _chips.Add(chip);
        }
    }

    private void HandleLinkComplete(List<LinkableBase> linkables)
    {
        Debug.Log($"Link complete: {linkables.Count}");
        
        // TODO: Implement chip destruction and scoring logic
        // For now, just log the positions
        foreach (var linkable in linkables)
        {
            Debug.Log($"Matched chip at ({linkable.Tile.Row}, {linkable.Tile.Column})");
        }
    }

    private void DestroyChips()
    {
        if (_chips == null)
        {
            Debug.LogWarning("Chips list is null");
            return;
        }
        
        var chipsToDestroy = _chips.Where(chip => chip != null).ToList();
        
        if (chipsToDestroy.Count == 0)
        {
            Debug.LogWarning("No chips to destroy");
            return;
        }

        for (var i = chipsToDestroy.Count - 1; i >= 0; i--)
        {
            var chip = chipsToDestroy[i];
            if (chip == null)
            {
                Debug.LogWarning($"Chip is null at {i}");
                continue;
            }
            
            chip.Release();
            _boardSystem.RemoveOccupant(chip);
            Destroy(chip.gameObject);
        }
    }
}
