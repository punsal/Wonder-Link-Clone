using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.Board.Abstract;
using Core.Board.Tile.Abstract;
using Core.Camera;
using Core.Camera.Abstract;
using Core.Camera.Provider;
using Core.Camera.Provider.Abstract;
using Core.Event;
using Core.Link;
using Core.Link.Abstract;
using Core.Link.Interface;
using Core.Runner.Interface;
using Gameplay.Chip;
using Gameplay.Chip.Abstract;
using Gameplay.Input;
using Gameplay.Input.Abstract;
using Gameplay.Systems.BoardRefill;
using Gameplay.Systems.BoardRefill.Abstract;
using Gameplay.Systems.MatchDetection;
using Gameplay.Systems.MatchDetection.Abstract;
using Gameplay.Systems.Score;
using Gameplay.Systems.Score.Abstract;
using Gameplay.Systems.Score.Observable;
using Gameplay.Systems.Shuffle;
using Gameplay.Systems.Shuffle.Abstract;
using Gameplay.Systems.Turn;
using Gameplay.Systems.Turn.Abstract;
using Gameplay.Systems.Turn.Observable;
using UnityEngine;

public class GameManager : MonoBehaviour, ICoroutineRunner
{
    [Header("Camera")]
    [SerializeField] private UnityCameraProviderBase gameCameraProvider;
    
    [Header("Board")]
    [SerializeField, Range(4, 12)] private int rowCount = 8;
    [SerializeField, Range(4, 12)] private int columnCount = 8;
    [SerializeField] private TileBase boardTilePrefab;
    
    [Header("Chip")]
    [SerializeField] private List<ChipBase> chipPrefabs;
    
    [Header("Linking")]
    [SerializeField] private UnityCameraProviderBase linkCameraProvider;
    [SerializeField] private LayerMask linkLayerMask;
    
    [Header("Gameplay")]
    [SerializeField] private int maxLevelScore = 100;
    [SerializeField] private int maxPlayerTurn = 10;
    
    [Header("System Configuration")]
    [SerializeField, Range(2, 5)] private int shuffleCountBeforeFailure = 2;
    [SerializeField] private GameScore levelScore;
    [SerializeField] private GameScore playerScore;
    [SerializeField] private GameTurn playerTurn;
    
    [Header("Events")]
    [SerializeField] private GameEvent startGameEvent;
    [SerializeField] private GameEvent noMoreTurnsEvent;
    [SerializeField] private GameEvent levelCompletedEvent;
    [SerializeField] private GameEvent shuffleFailedEvent;
    [SerializeField] private GameEvent nextGameEvent;

    private CameraSystemBase _cameraSystem;
    private BoardSystemBase _boardSystem;
    private LinkSystemBase _linkSystem;
    private ChipManagerBase _chipManager;
    private BoardRefillSystemBase _boardRefillSystem;
    private MatchDetectionSystemBase _matchDetectionSystem;
    private ShuffleSystemBase _shuffleSystem;
    private ScoreSystemBase _scoreSystem;
    private TurnSystemBase _turnSystem;
    private InputHandlerBase _inputHandler;
    
    private int _currentShuffleCount;
    
    private void Awake()
    {
        CreateSystems();
    }
    
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Start()
    {
        CreateGameplay();
    }

    private void Update()
    {
        _inputHandler?.Update();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        DestroySystems();
    }

    private void CreateSystems()
    {
        CreateGameCameraSystem();
        CreateBoardSystem();
        CreateLinkSystem();
        CreateChipManager();
        CreateBoardRefillSystem();
        CreateMatchDetectionSystem();
        CreateShuffleSystem();
        CreateScoreSystem();
        CreateTurnSystem();
        CreateInputHandler();   
    }

    private void CreateGameplay()
    {
        _currentShuffleCount = 0;
        
        _boardSystem.Initialize();
        _chipManager.FillBoard();
        _cameraSystem.CenterOnBoard(rowCount, columnCount);

        if (!ShouldShuffle())
        {
            Debug.Log("Board has moves, no need to shuffle");
            return;
        }
        
        Debug.Log("Starting shuffle");
        _shuffleSystem.StartShuffle();
    }

    private void DestroySystems()
    {
        _turnSystem.Dispose();
        _scoreSystem.Dispose();
        _shuffleSystem.Dispose();
        _boardRefillSystem.Dispose();
        _chipManager.Dispose();
        _linkSystem.Dispose();
        _boardSystem.Dispose();
    }

    private void SubscribeToEvents()
    {
        _linkSystem.OnLinkCompleted += HandleLinkCompleted;
        _boardRefillSystem.OnRefillCompleted += HandleRefillCompleted;
        _shuffleSystem.OnShuffleCompleted += HandleShuffleCompleted;
        
        startGameEvent.OnEventRaised += HandleGameStartEvent;
        nextGameEvent.OnEventRaised += HandleNextGameEvent;
    }

    private void UnsubscribeFromEvents()
    {
        _linkSystem.OnLinkCompleted -= HandleLinkCompleted;
        _boardRefillSystem.OnRefillCompleted -= HandleRefillCompleted;
        _shuffleSystem.OnShuffleCompleted -= HandleShuffleCompleted;
        
        startGameEvent.OnEventRaised -= HandleGameStartEvent;
        nextGameEvent.OnEventRaised -= HandleNextGameEvent;
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
        if (boardTilePrefab == null)
        {
            Debug.LogError("Tile prefab is null");
        }
        _boardSystem = new BoardSystem(rowCount, columnCount, boardTilePrefab);
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
        else
        {
            _linkSystem = new LinkSystem(linkCameraProvider, linkLayerMask);
        }
    }

    private void CreateChipManager()
    {
        if (chipPrefabs == null || chipPrefabs.Count == 0)
        {
            Debug.LogError("No chip prefabs found");
        }
        _chipManager = new ChipManager(_boardSystem, chipPrefabs);
    }

    private void CreateBoardRefillSystem()
    {
        _boardRefillSystem = new BoardRefillSystem(_boardSystem, _chipManager, this);
    }

    private void CreateMatchDetectionSystem()
    {
        _matchDetectionSystem = new MatchDetectionSystem(_boardSystem, _chipManager);
    }

    private void CreateShuffleSystem()
    {
        _shuffleSystem = new ShuffleSystem(_boardSystem, _chipManager, this);
    }

    private void CreateScoreSystem()
    {
        if (!levelScore)
        {
            Debug.LogError("Level score is null, creating default");
            levelScore = ScriptableObject.CreateInstance<GameScore>();
        }
        
        levelScore.SetValue(maxLevelScore);

        if (!playerScore)
        {
            Debug.LogError("Player score is null, creating default");
            playerScore = ScriptableObject.CreateInstance<GameScore>();
        }
        
        playerScore.SetValue(0);
        
        _scoreSystem = new ScoreSystem(levelScore, playerScore);
    }

    private void CreateTurnSystem()
    {
        if (!playerTurn)
        {
            Debug.LogError("Player turn is null, creating default");
            playerTurn = ScriptableObject.CreateInstance<GameTurn>();
        }

        _turnSystem = new TurnSystem(playerTurn, maxPlayerTurn);
    }

    private void CreateInputHandler()
    {
#if UNITY_EDITOR
        _inputHandler = new MouseInputHandler(_linkSystem);
        Debug.Log("Using MouseInputHandler in Unity Editor");
#elif UNITY_ANDROID || UNITY_IOS
        _inputHandler = new TouchInputHandler(_linkSystem);
        Debug.Log("Using TouchInputHandler for mobile platform");
#else
        _inputHandler = new MouseInputHandler(_linkSystem);
        Debug.Log("Using MouseInputHandler for other platforms");
#endif
        _inputHandler.Disable();
    }

    private void HandleLinkCompleted(List<ILinkable> linkables)
    {
        Debug.Log($"Link complete: {linkables.Count} chips matched");
        
        // Disable input during refill sequence
        _inputHandler.Disable();
        
        // update turn count
        _turnSystem.FinishTurn();
        
        // copy linkables to a new list of chips to process
        var chips = linkables.Cast<ChipBase>().ToList();

        // update score
        foreach (var chip in chips)
        {
            _scoreSystem.AddScore(chip.Score);
        }
        
        // Start destruction and refill sequence
        _boardRefillSystem.StartRefill(chips);
    }

    private void HandleRefillCompleted()
    {
        // check if the score is fulfilled
        if (_scoreSystem.IsScoreReached)
        {
            Debug.Log("Level completed!");
            levelCompletedEvent.Raise();
            return;
        }
        
        // check if there are remaining turns
        if (!_turnSystem.IsTurnAvailable)
        {
            Debug.Log("No more turns available");
            noMoreTurnsEvent.Raise();
            return;
        }
        
        if (ShouldShuffle())
        {
            Debug.Log("No possible moves detected");
            _shuffleSystem.StartShuffle();
            return;
        }
        
        Debug.Log("Refill complete");
        _inputHandler.Enable();
    }

    private bool ShouldShuffle()
    {
        if (_matchDetectionSystem.HasPossibleMoves())
        {
            return false;
        }
        _currentShuffleCount = 0;
        return true;
    }

    private void HandleShuffleCompleted()
    {
        _currentShuffleCount++;
        Debug.Log($"Current shuffle count: {_currentShuffleCount}");
        
        if (!_matchDetectionSystem.HasPossibleMoves())
        {
            if (_currentShuffleCount < shuffleCountBeforeFailure)
            {
                Debug.Log("No possible moves detected, shuffle failed. Trying again.");
                _shuffleSystem.StartShuffle();
                return;
            }
            
            Debug.Log("No possible moves detected, shuffle failed. No more attempts.");
            shuffleFailedEvent.Raise();
            return;
        }
        
        Debug.Log("Shuffle succeed.");
        _inputHandler.Enable();
    }

    private void HandleGameStartEvent()
    {
        _inputHandler.Enable();
    }

    private void HandleNextGameEvent()
    {
        Replay();
    }

    private void Replay()
    {
        Debug.Log("Stopping current game, destroying systems");
        UnsubscribeFromEvents();
        DestroySystems();
        
        Debug.Log("Starting next game");
        CreateSystems();
        SubscribeToEvents();
        CreateGameplay();
        
        Debug.Log("Next game ready");
    }
    
    [ContextMenu("Debug Chip Count")]
    private void DebugChipCount()
    {
        var activeChips = _chipManager.ActiveChips;
        var nonNullChips = activeChips.Where(c => c != null).ToList();
        var withTiles = nonNullChips.Where(c => c.Tile != null).ToList();
    
        Debug.Log($"=== CHIP COUNT DEBUG ===");
        Debug.Log($"Total in list: {activeChips.Count}");
        Debug.Log($"Non-null: {nonNullChips.Count}");
        Debug.Log($"With tiles: {withTiles.Count}");
        Debug.Log($"Expected: {rowCount * columnCount}");
    
        // Check for duplicates
        var duplicates = withTiles
            .GroupBy(c => (c.Tile.Row, c.Tile.Column))
            .Where(g => g.Count() > 1)
            .ToList();
    
        if (duplicates.Any())
        {
            Debug.LogError($"Found {duplicates.Count} duplicate positions:");
            foreach (var dup in duplicates)
            {
                Debug.LogError($"  Position ({dup.Key.Row}, {dup.Key.Column}): {dup.Count()} chips - {string.Join(", ", dup.Select(c => c.name))}");
            }
        }
        else
        {
            Debug.Log("No duplicates found!");
        }
    }
}
