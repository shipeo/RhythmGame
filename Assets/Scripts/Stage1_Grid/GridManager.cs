using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    
    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 8;
    [SerializeField] private float cellSize = 80f;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private Color cellColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color noteColor = Color.cyan;
    
    private GridCell[,] cells;
    
    void Awake() { Instance = this; }
    
    void Start()
    {
        CreateGrid();
        Invoke(nameof(SpawnTest1), 2f);
        Invoke(nameof(SpawnTest2), 3f);
        Invoke(nameof(SpawnTest3), 4f);
    }
    
    void CreateGrid()
    {
        var canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        var grid = new GameObject("Grid");
        grid.transform.SetParent(canvas.transform, false);
        var gridRT = grid.AddComponent<RectTransform>();
        gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        
        float total = gridSize * (cellSize + spacing) - spacing;
        gridRT.sizeDelta = new Vector2(total, total);
        
        cells = new GridCell[gridSize, gridSize];
        float start = -total / 2 + cellSize / 2;
        
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                var cellObj = new GameObject($"Cell_{r}_{c}");
                cellObj.transform.SetParent(grid.transform, false);
                
                var rt = cellObj.AddComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(
                    start + c * (cellSize + spacing),
                    -start - r * (cellSize + spacing)
                );
                
                cellObj.AddComponent<UnityEngine.UI.Image>().color = cellColor;
                
                var cell = cellObj.AddComponent<GridCell>();
                cell.Setup(r, c, noteColor);
                cells[r, c] = cell;
            }
        }
        Debug.Log($"Grid {gridSize}x{gridSize} ready!");
    }
    
    void SpawnTest1()
    {
        cells[0,0].SpawnNote();
        cells[0,7].SpawnNote();
        cells[7,0].SpawnNote();
        cells[7,7].SpawnNote();
    }
    
    void SpawnTest2()
    {
        cells[3,3].SpawnNote();
        cells[3,4].SpawnNote();
        cells[4,3].SpawnNote();
        cells[4,4].SpawnNote();
    }
    
    void SpawnTest3()
    {
        for (int i = 0; i < 8; i++)
            cells[i, i].SpawnNote();
    }
    
    public void OnCellTapped(int r, int c)
    {
        if (cells[r,c].HasNote)
        {
            var result = ScoreManager.Instance.JudgeHit(0.05f);
            cells[r,c].HitNote(result);
            Debug.Log($"HIT ({r},{c}): {result}");
        }
    }
    
    public void InitializeGrid(ChartData.StageData data)
    {
        gridSize = data.gridSize;
    }
}
