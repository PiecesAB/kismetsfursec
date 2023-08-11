using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMapPositioner : MonoBehaviour {

	// Use this for initialization
	void Start () {
       LevelInfoContainer l = FindObjectOfType<LevelInfoContainer>();
        RawImage img = GetComponent<RawImage>();
        Vector4 rs = new Vector4(l.mapPosition.x, l.mapPosition.y, l.mapPosition.width, l.mapPosition.height);
        Vector2 imgp = new Vector2(
            Mathf.Clamp(  ((l.mapPosition.x / 160f) + (l.mapPosition.width/320f)) - (img.uvRect.size.x / 2f) ,0f,1f- img.uvRect.size.x),
            Mathf.Clamp(((l.mapPosition.y / 160f) + (l.mapPosition.height / 320f)) - (img.uvRect.size.y / 2f), 0f, 1f - img.uvRect.size.y)
            );
        img.uvRect = new Rect(imgp,img.uvRect.size);
        img.material.SetVector("_Rect", rs);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
