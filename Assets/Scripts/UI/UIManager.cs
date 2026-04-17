using UnityEngine;

/// <summary>
/// UIManager - Handles all UI elements
/// TODO: Implement UI functionality
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowPauseMenu()
    {
        Debug.Log("UIManager: Show pause menu (TODO)");
    }

    public void HidePauseMenu()
    {
        Debug.Log("UIManager: Hide pause menu (TODO)");
    }
}
