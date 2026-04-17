using UnityEngine;

[CreateAssetMenu(fileName = "NewChart", menuName = "Rhythm Game/Chart Data")]
public class ChartData : ScriptableObject
{
    public string songName;
    public string artist;
    public AudioClip audioClip;
    public float bpm = 120f;
    public StageData[] stages;

    [System.Serializable]
    public class StageData
    {
        public StageType type;
        public float startTime;
        public float endTime;
        public NoteData[] notes;
        public int gridSize = 8;
    }

    public enum StageType { Grid, Scroll }
}

[System.Serializable]
public class NoteData
{
    public float time;
    public NoteType type;
    public Vector2 position;
    public float duration;
    public Vector2 endPosition;
    
    public enum NoteType { Tap, Hold, Slide, Drag }
}
