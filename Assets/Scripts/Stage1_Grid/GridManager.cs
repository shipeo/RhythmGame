using UnityEngine;

/// <summary>
/// GridManager - Handles Stage 1 grid gameplay
/// TODO: Implement 8x8 grid system
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeGrid(ChartData.StageData stageData)
    {
        Debug.Log($"GridManager: Initialize grid (size: {stageData.gridSize}x{stageData.gridSize})");
        // TODO: Create grid, spawn notes
    }
}
