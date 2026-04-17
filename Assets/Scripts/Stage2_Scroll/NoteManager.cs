using UnityEngine;

/// <summary>
/// NoteManager - Handles Stage 2 free-positioning notes
/// TODO: Implement Cytus-style note spawning
/// </summary>
public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartStage2(ChartData chart)
    {
        Debug.Log("NoteManager: Start Stage 2");
        // TODO: Initialize scanner, spawn notes
    }
}
