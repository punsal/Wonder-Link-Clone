using System.Collections.Generic;
using System.Linq;
using Board;
using Board.Abstract;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    [SerializeField] private Tile tilePrefab;
    
    [Header("Chip")]
    [SerializeField] private Chip[] chipPrefabs;

    private BoardBase _board;
    private List<Chip> _chips;
    
    private void Awake()
    {
        _board = new GameBoard(rowCount, columnCount, tilePrefab);
        _chips = new List<Chip>();
    }
    
    private void OnEnable()
    {
        _board.Initialize();
        CreateChips();
    }
    
    private void OnDisable()
    {
        DestroyChips();
        _board.Dispose();
    }

    private void CreateChips()
    {
        var chipPrefabsCount = chipPrefabs.Length;
        var boardSize = rowCount * columnCount;
        for (var i = 0; i < boardSize; i++)
        {
            if (!_board.TryGetEmptyTile(out var emptyTile))
            {
                Debug.LogWarning("No empty tiles");
                break;
            }
            var randomIndex = Random.Range(0, chipPrefabsCount);
            var chipPrefab = chipPrefabs[randomIndex];
            var chip = Instantiate(chipPrefab, Vector3.zero, Quaternion.identity);
            chip.Occupy(emptyTile);
            _board.AddOccupant(chip);
            _chips.Add(chip);
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
            _board.RemoveOccupant(chip);
            Destroy(chip.gameObject);
        }
    }
}
