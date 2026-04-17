using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridCell : MonoBehaviour, IPointerDownHandler
{
    private int row, col;
    private Image image;
    private Color normalColor;
    private Color noteActiveColor;
    private GameObject noteObj;
    private float pulseTime;
    private bool isHit = false;
    
    public bool HasNote => noteObj != null && noteObj.activeInHierarchy && !isHit;
    
    public void Setup(int r, int c, Color noteColor)
    {
        row = r;
        col = c;
        image = GetComponent<Image>();
        normalColor = image.color;
        noteActiveColor = noteColor;
    }
    
    public void SpawnNote()
    {
        if (noteObj != null) return;
        
        noteObj = new GameObject("Note");
        noteObj.transform.SetParent(transform, false);
        
        var rt = noteObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = GetComponent<RectTransform>().sizeDelta * 0.8f;
        rt.anchoredPosition = Vector2.zero;
        
        var img = noteObj.AddComponent<Image>();
        img.color = noteActiveColor;
        
        pulseTime = 0f;
        isHit = false;
    }
    
    void Update()
    {
        if (noteObj != null && noteObj.activeInHierarchy && !isHit)
        {
            pulseTime += Time.deltaTime * 2f;
            float scale = 1f + Mathf.Sin(pulseTime) * 0.2f;
            noteObj.transform.localScale = Vector3.one * scale;
        }
    }
    
    public void HitNote(ScoreManager.JudgementType result)
    {
        if (noteObj == null || !noteObj.activeInHierarchy || isHit) return;
        
        isHit = true;
        
        Color feedbackColor = result switch
        {
            ScoreManager.JudgementType.Perfect => Color.yellow,
            ScoreManager.JudgementType.Good => Color.green,
            _ => Color.red
        };
        
        var img = noteObj.GetComponent<Image>();
        img.color = feedbackColor;
        
        StartCoroutine(ExplodeEffect());
    }
    
    System.Collections.IEnumerator ExplodeEffect()
    {
        if (noteObj == null) yield break;
        
        float time = 0f;
        Vector3 startScale = noteObj.transform.localScale;
        Image img = noteObj.GetComponent<Image>();
        
        while (time < 0.3f && noteObj != null)
        {
            time += Time.deltaTime;
            float t = time / 0.3f;
            
            if (noteObj != null)
            {
                noteObj.transform.localScale = startScale * (1f + t * 0.5f);
                
                var col = img.color;
                col.a = 1f - t;
                img.color = col;
            }
            
            yield return null;
        }
        
        if (noteObj != null)
        {
            Destroy(noteObj);
            noteObj = null;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        GridManager.Instance?.OnCellTapped(row, col);
        
        image.color = Color.white;
        StartCoroutine(FlashEffect());
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        yield return new WaitForSeconds(0.1f);
        if (image != null)
            image.color = normalColor;
    }
}
