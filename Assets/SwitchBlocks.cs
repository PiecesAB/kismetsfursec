using UnityEngine;
using System.Collections;
using System;

public class SwitchBlocks : MonoBehaviour {

    public GameObject distortionEffectMesh;
    public MonoBehaviour[] scriptsToEnable;
    [Range(0, 31)]
    public int switchID;
    public bool inverted;
    public bool on;
    private uint actualID;


   public void Changed(uint nmask)
    {
        if ((!inverted && (actualID & Utilities.loadedSaveData.switchMask) != 0) || (inverted && (actualID & Utilities.loadedSaveData.switchMask) == 0))
        {
            distortionEffectMesh.SetActive(false);
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = true;
            }
            GetComponent<SpriteRenderer>().color = Color.white;
            foreach (MonoBehaviour scr in scriptsToEnable)
            {
                scr.enabled = true;
            }
        }
        else
        {
            distortionEffectMesh.SetActive(true);
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.375f);
            foreach (MonoBehaviour scr in scriptsToEnable)
            {
                scr.enabled = false;
            }
        }
    }

	// Use this for initialization
	void Awake () {
        actualID = 1u << switchID;
	}
	
    void Start ()
    {
        actualID = 1u << switchID;
        if ((!inverted && (actualID & Utilities.loadedSaveData.switchMask) != 0) || (inverted && (actualID & Utilities.loadedSaveData.switchMask) == 0))
        {
            distortionEffectMesh.SetActive(false);
            GetComponent<Collider2D>().enabled = true;
            GetComponent<SpriteRenderer>().color = Color.white;
            foreach (MonoBehaviour scr in scriptsToEnable)
            {
                scr.enabled = true;
            }
        }
        else
        {
            distortionEffectMesh.SetActive(true);
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.375f);
            foreach (MonoBehaviour scr in scriptsToEnable)
            {
                scr.enabled = false;
            }
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
