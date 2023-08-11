using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsSelectionBox : MonoBehaviour {

    public SettingsOptionObject[] these;
    public MenuScrollHandler scroll;
    public Transform inset;
    public bool scrollEnabled = true;

    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        int n = scroll.index;
        RectTransform t = these[n].title.GetComponent<RectTransform>();
        rt.position = Vector3.Lerp(rt.position, t.position, 0.25f);
        rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, t.sizeDelta + new Vector2(2f, 2f), 0.3f);
        if (scrollEnabled)
        {
            if (rt.localPosition.y < 0) { inset.localPosition = new Vector3(0, -rt.localPosition.y, 0); }
            else { inset.localPosition = Vector3.zero; }
        }
    }
}
