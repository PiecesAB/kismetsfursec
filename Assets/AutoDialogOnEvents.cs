using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoDialogOnEvents : MonoBehaviour {

    public enum Reason
    {
        DefaultWhenLevelBegins,Deaths,
    }

    [System.Serializable]
    public struct Gruep
    {
        public GameObject boxObject;
        public Reason reason;
        public float var1quota;
        public float var2delay;
    }

    private bool wentBegin = false;
    public List<Gruep> stuff = new List<Gruep>();

	void Start () {
        wentBegin = false;
	}
	
	void Update () {
        foreach (var i in stuff)
        {
            if (i.reason == Reason.Deaths)
            {
                if (i.var2delay <= DoubleTime.ScaledTimeSinceLoad && Utilities.loadedSaveData.deathsThisLevel >= i.var1quota && !wentBegin && Utilities.canPauseGame)
                {
                    GameObject made = Instantiate(i.boxObject);
                    made.SetActive(true);
                    made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                    wentBegin = true;
                    stuff.Remove(i);
                    break;
                }
            }
            if (i.reason == Reason.DefaultWhenLevelBegins)
            {
                if (i.var2delay <= DoubleTime.ScaledTimeSinceLoad && !wentBegin && Utilities.canPauseGame)
                {
                    GameObject made = Instantiate(i.boxObject);
                    made.SetActive(true);
                    made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                    wentBegin = true;
                    stuff.Remove(i);
                    break;
                }
            }
        }
	}
}
