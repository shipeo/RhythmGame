using UnityEngine;
using System;

/// <summary>
/// ScoreManager - Handles scoring system: Perfect/Good/Miss + Combo
/// Cross-platform compatible
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Timing Windows (in seconds)")]
    [SerializeField] private float perfectWindow = 0.08f;  // 80ms
    [SerializeField] private float goodWindow = 0.15f;     // 150ms

    [Header("Score Values")]
    [SerializeField] private int perfectScore = 100;
    [SerializeField] private int goodScore = 50;

    // Current stats
    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int perfectCount = 0;
    private int goodCount = 0;
    private int missCount = 0;

    // Events
    public static event Action<JudgementType, int> OnJudgement;
    public static event Action<int> OnComboChanged;
    public static event Action<int> OnScoreChanged;

    public enum JudgementType { Perfect, Good, Miss }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        perfectCount = goodCount = missCount = 0;
    }

    /// <summary>
    /// Judge note hit based on timing difference
    /// </summary>
    public JudgementType JudgeHit(float timingDifference)
    {
        float absDiff = Mathf.Abs(timingDifference);
        JudgementType judgement;

        if (absDiff <= perfectWindow)
        {
            judgement = JudgementType.Perfect;
            currentScore += perfectScore;
            perfectCount++;
            currentCombo++;
        }
        else if (absDiff <= goodWindow)
        {
            judgement = JudgementType.Good;
            currentScore += goodScore;
            goodCount++;
            currentCombo++;
        }
        else
        {
            judgement = JudgementType.Miss;
            missCount++;
            currentCombo = 0;
        }

        if (currentCombo > maxCombo)
            maxCombo = currentCombo;

        OnJudgement?.Invoke(judgement, currentCombo);
        OnComboChanged?.Invoke(currentCombo);
        OnScoreChanged?.Invoke(currentScore);

        return judgement;
    }

    public void RegisterMiss()
    {
        missCount++;
        currentCombo = 0;
        OnJudgement?.Invoke(JudgementType.Miss, 0);
        OnComboChanged?.Invoke(0);
    }

    // Getters
    public int CurrentScore => currentScore;
    public int CurrentCombo => currentCombo;
    public int MaxCombo => maxCombo;
    public int PerfectCount => perfectCount;
    public int GoodCount => goodCount;
    public int MissCount => missCount;
    public float Accuracy => CalculateAccuracy();

    private float CalculateAccuracy()
    {
        int totalNotes = perfectCount + goodCount + missCount;
        if (totalNotes == 0) return 0f;
        return (float)(perfectCount + goodCount) / totalNotes * 100f;
    }
}
