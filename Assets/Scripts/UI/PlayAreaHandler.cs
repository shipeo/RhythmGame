using UnityEngine;

/// <summary>
/// PlayAreaHandler - Manages safe zones for iPhone notch/Dynamic Island
/// CRITICAL for iOS compatibility
/// Automatically adjusts to Screen.safeArea on all platforms
/// </summary>
public class PlayAreaHandler : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [SerializeField, Range(0f, 0.3f)]
    private float additionalTopPadding = 0.05f;  // Extra 5% top padding
    
    [SerializeField, Range(0f, 0.3f)]
    private float additionalBottomPadding = 0.05f;  // Extra 5% bottom padding

    private Rect safeArea;
    private RectTransform rectTransform;

    // Public properties for other scripts to query safe play area
    public Rect SafePlayArea { get; private set; }
    public Vector2 PlayAreaMin { get; private set; }
    public Vector2 PlayAreaMax { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        // Re-apply if screen orientation changes (for testing)
        if (safeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        safeArea = Screen.safeArea;

        // Convert safe area to anchors
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply additional padding
        anchorMin.y += additionalBottomPadding;
        anchorMax.y -= additionalTopPadding;

        // Clamp to valid range
        anchorMin = Vector2.Max(anchorMin, Vector2.zero);
        anchorMax = Vector2.Min(anchorMax, Vector2.one);

        // Apply to RectTransform
        if (rectTransform != null)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        // Calculate play area bounds for gameplay logic
        CalculatePlayAreaBounds();

        Debug.Log($"Safe Area Applied: {anchorMin} to {anchorMax}");
    }

    private void CalculatePlayAreaBounds()
    {
        // Get actual pixel bounds of safe area
        Vector2 min = new Vector2(
            safeArea.xMin + Screen.width * additionalBottomPadding,
            safeArea.yMin + Screen.height * additionalBottomPadding
        );
        
        Vector2 max = new Vector2(
            safeArea.xMax - Screen.width * additionalTopPadding,
            safeArea.yMax - Screen.height * additionalTopPadding
        );

        PlayAreaMin = min;
        PlayAreaMax = max;
        SafePlayArea = new Rect(min, max - min);
    }

    /// <summary>
    /// Check if a screen position is inside the safe play area
    /// Useful for spawning notes
    /// </summary>
    public bool IsPositionInPlayArea(Vector2 screenPosition)
    {
        return SafePlayArea.Contains(screenPosition);
    }

    /// <summary>
    /// Convert normalized position (0-1) to screen position within play area
    /// Use this when spawning notes from chart data
    /// </summary>
    public Vector2 NormalizedToScreenPosition(Vector2 normalized)
    {
        return new Vector2(
            Mathf.Lerp(PlayAreaMin.x, PlayAreaMax.x, normalized.x),
            Mathf.Lerp(PlayAreaMin.y, PlayAreaMax.y, normalized.y)
        );
    }

    /// <summary>
    /// Get play area center (useful for grid centering)
    /// </summary>
    public Vector2 GetPlayAreaCenter()
    {
        return (PlayAreaMin + PlayAreaMax) * 0.5f;
    }

    /// <summary>
    /// Get play area size in pixels
    /// </summary>
    public Vector2 GetPlayAreaSize()
    {
        return PlayAreaMax - PlayAreaMin;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Vector3 min3D = Camera.main.ScreenToWorldPoint(new Vector3(PlayAreaMin.x, PlayAreaMin.y, 10f));
        Vector3 max3D = Camera.main.ScreenToWorldPoint(new Vector3(PlayAreaMax.x, PlayAreaMax.y, 10f));
        
        Gizmos.DrawLine(min3D, new Vector3(max3D.x, min3D.y, min3D.z));
        Gizmos.DrawLine(new Vector3(max3D.x, min3D.y, min3D.z), max3D);
        Gizmos.DrawLine(max3D, new Vector3(min3D.x, max3D.y, max3D.z));
        Gizmos.DrawLine(new Vector3(min3D.x, max3D.y, max3D.z), min3D);
    }
}
