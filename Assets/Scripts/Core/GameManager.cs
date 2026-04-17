using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// GameManager - Main controller for the rhythm game
/// 
/// Responsibilities:
/// - Manage game state (Menu, Stage1, Stage2, Paused, GameOver)
/// - Handle stage transitions based on BPM/timing
/// - Coordinate between all other managers (Audio, Score, UI)
/// - Initialize and cleanup game systems
/// 
/// Usage: Attach to a GameObject named "GameManager" in your scene
/// This should be a singleton - only one instance in the scene
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton Pattern
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }
    #endregion

    #region Game State
    public enum GameState
    {
        Menu,           // Main menu / song selection
        Loading,        // Loading chart and audio
        Stage1_Grid,    // 8x8 grid tap gameplay
        Stage2_Scroll,  // Free-positioning Cytus-style gameplay
        Paused,         // Game paused
        GameOver,       // Song finished, show results
    }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    public GameState CurrentState => currentState;

    // State change event - other systems can subscribe to this
    public delegate void OnStateChange(GameState newState);
    public static event OnStateChange StateChanged;
    #endregion

    #region References to Other Managers
    [Header("Manager References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;

    // Stage managers (will be enabled/disabled based on current stage)
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NoteManager noteManager;
    #endregion

    #region Chart Data
    [Header("Current Song")]
    [SerializeField] private ChartData currentChart;
    public ChartData CurrentChart => currentChart;

    private float songStartTime;      // When the song started (Time.time)
    private float currentSongTime => Time.time - songStartTime;  // Current position in song
    #endregion

    #region Stage Transition Settings
    [Header("Stage Transition")]
    [Tooltip("Will be set automatically from chart data")]
    [SerializeField] private float stage1Duration = 30f;  // Duration of Stage 1 (from chart)
    [SerializeField] private float transitionDelay = 0.5f; // Delay before transitioning to Stage 2
    
    private bool hasTransitionedToStage2 = false;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Initialize all systems
        InitializeManagers();
        
        // Start at menu state
        ChangeState(GameState.Menu);
    }

    private void Update()
    {
        // Handle state-specific logic
        switch (currentState)
        {
            case GameState.Stage1_Grid:
                UpdateStage1();
                break;
                
            case GameState.Stage2_Scroll:
                UpdateStage2();
                break;
        }

        // Check for pause input (ESC or dedicated pause button)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == GameState.Stage1_Grid || currentState == GameState.Stage2_Scroll)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
    #endregion

    #region Initialization
    private void InitializeManagers()
    {
        // Find managers if not assigned in inspector
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (noteManager == null)
            noteManager = FindObjectOfType<NoteManager>();

        // Verify all managers exist
        if (audioManager == null) Debug.LogError("AudioManager not found!");
        if (scoreManager == null) Debug.LogError("ScoreManager not found!");
        if (uiManager == null) Debug.LogError("UIManager not found!");
        if (gridManager == null) Debug.LogError("GridManager not found!");
        if (noteManager == null) Debug.LogError("NoteManager not found!");

        Debug.Log("GameManager: All systems initialized");
    }
    #endregion

    #region State Management
    public void ChangeState(GameState newState)
    {
        // Exit current state
        ExitState(currentState);

        // Change state
        GameState previousState = currentState;
        currentState = newState;

        // Enter new state
        EnterState(newState);

        // Notify listeners
        StateChanged?.Invoke(newState);

        Debug.Log($"GameManager: State changed from {previousState} to {newState}");
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                // Show menu UI
                Time.timeScale = 1f;
                break;

            case GameState.Stage1_Grid:
                // Enable grid gameplay
                gridManager?.gameObject.SetActive(true);
                noteManager?.gameObject.SetActive(false);
                
                // Start music
                audioManager?.PlaySong(currentChart.audioClip);
                songStartTime = Time.time;
                hasTransitionedToStage2 = false;
                
                // Reset score
                scoreManager?.ResetScore();
                break;

            case GameState.Stage2_Scroll:
                // Disable grid, enable scroll
                gridManager?.gameObject.SetActive(false);
                noteManager?.gameObject.SetActive(true);
                noteManager?.StartStage2(currentChart);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                audioManager?.PauseMusic();
                break;

            case GameState.GameOver:
                // Show results screen
                audioManager?.StopMusic();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                Time.timeScale = 1f;
                break;
        }
    }
    #endregion

    #region Stage 1 Update
    private void UpdateStage1()
    {
        // Check if it's time to transition to Stage 2
        if (!hasTransitionedToStage2 && currentSongTime >= stage1Duration)
        {
            hasTransitionedToStage2 = true;
            TransitionToStage2();
        }
    }

    private void TransitionToStage2()
    {
        Debug.Log("GameManager: Transitioning to Stage 2");
        
        // Optional: Add transition effect here (fade, animation, etc.)
        // For now, direct transition after small delay
        
        Invoke(nameof(StartStage2), transitionDelay);
    }

    private void StartStage2()
    {
        ChangeState(GameState.Stage2_Scroll);
    }
    #endregion

    #region Stage 2 Update
    private void UpdateStage2()
    {
        // Check if song is finished
        if (audioManager != null && !audioManager.IsMusicPlaying())
        {
            EndSong();
        }
    }

    private void EndSong()
    {
        Debug.Log("GameManager: Song finished");
        ChangeState(GameState.GameOver);
    }
    #endregion

    #region Pause / Resume
    public void PauseGame()
    {
        if (currentState == GameState.Stage1_Grid || currentState == GameState.Stage2_Scroll)
        {
            ChangeState(GameState.Paused);
            uiManager?.ShowPauseMenu();
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            // Return to previous gameplay state
            // TODO: Track previous state properly
            ChangeState(GameState.Stage1_Grid); // Simplified for now
            audioManager?.ResumeMusic();
            uiManager?.HidePauseMenu();
        }
    }
    #endregion

    #region Public API for Starting Game
    /// <summary>
    /// Call this to start playing a song with a specific chart
    /// </summary>
    public void StartSong(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogError("GameManager: Cannot start song with null chart!");
            return;
        }

        currentChart = chart;
        
        // Get stage 1 duration from chart data
        if (chart.stages != null && chart.stages.Length > 0)
        {
            stage1Duration = chart.stages[0].endTime;
        }

        // Initialize grid with chart data
        if (gridManager != null)
        {
            gridManager.InitializeGrid(chart.stages[0]);
        }

        // Start the game at Stage 1
        ChangeState(GameState.Stage1_Grid);
    }
    #endregion

    #region Debug / Testing
    // Call this from Inspector or debug menu to test
    [ContextMenu("Test Start Song")]
    private void TestStartSong()
    {
        if (currentChart != null)
        {
            StartSong(currentChart);
        }
        else
        {
            Debug.LogWarning("GameManager: No chart assigned for testing!");
        }
    }
    #endregion
}
