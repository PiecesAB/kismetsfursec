using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class TitleMenuChoosing : MonoBehaviour {

    public int selected = 0;
    public List<GameObject> selections;
    public List<GameObject> textsForSelections;
    public Sprite[] animPieces;

    public bool[] additiveNextScenes;
    public string[] nextScenes;

    public Image transitionImage;

    private float t;
    private int a;
	// Use this for initialization
	void Start () {
        selected = 0;
        t = 0;
        a = 0;
        PlayerPrefs.SetString("gameIsOpen", "Yes.");
	}
	
    void NowSelect()
    {

    }

	// Update is called once per frame
	void Update () {

        {
            a = Mathf.Max(a - 1, 0);
            foreach (var ii in selections)
            {
                if (ii == selections[selected])
                {
                    Image iiCaptain = ii.GetComponent<Image>();
                    iiCaptain.sprite = animPieces[3 * selected + Mathf.FloorToInt((((float)DoubleTime.ScaledTimeSinceLoad - t) % 1f) * 3)];
                    Vector3 abc = textsForSelections[selections.IndexOf(ii)].transform.localPosition;
                    textsForSelections[selections.IndexOf(ii)].GetComponent<Image>().fillAmount = Mathf.Lerp(textsForSelections[selections.IndexOf(ii)].GetComponent<Image>().fillAmount, 1, 0.2f);
                    textsForSelections[selections.IndexOf(ii)].transform.localPosition = Vector3.Lerp(abc, new Vector3(96, abc.y, abc.z), 0.2f);
                    Vector3 tt = ii.transform.localPosition;
                    ii.transform.localPosition = Vector3.Lerp(tt, new Vector3(-88, tt.y, tt.z), 0.2f);
                }
                else
                {
                    Image iiCaptain = ii.GetComponent<Image>();
                    iiCaptain.sprite = animPieces[12 + selections.IndexOf(ii)];
                    Vector3 abc = textsForSelections[selections.IndexOf(ii)].transform.localPosition;
                    textsForSelections[selections.IndexOf(ii)].GetComponent<Image>().fillAmount = Mathf.Lerp(textsForSelections[selections.IndexOf(ii)].GetComponent<Image>().fillAmount, 0, 0.2f);
                    textsForSelections[selections.IndexOf(ii)].transform.localPosition = Vector3.Lerp(abc, new Vector3(-36, abc.y, abc.z), 0.2f);
                    Vector3 tt = ii.transform.localPosition;
                    ii.transform.localPosition = Vector3.Lerp(tt, new Vector3(-120, tt.y, tt.z), 0.2f);
                }
            }


            bool hi = false;
            //Dictionary<double, ulong> ud = GetComponent<Encontrolmentation>().eventsTable;
            Encontrolmentation en = GetComponent<Encontrolmentation>();
            var z = en.eventsTable[en.eventsTable.Count - 1];
            if ((GetComponent<Encontrolmentation>().flags & 4UL) == 4UL || (DoubleTime.UnscaledTimeRunning - z.Item1 > 0.7f && (z.Item2 & 4UL) == 4UL && a == 0))
            {
                t = (float)DoubleTime.ScaledTimeSinceLoad;
                a = Mathf.FloorToInt(Mathf.Max(25f - 4f * ((float)(DoubleTime.UnscaledTimeRunning - z.Item1)), 0f));
                int nselected = Mathf.Max(selected - 1, 0);
                if (nselected == selected)
                {
                    selected = 3;
                }
                else
                {
                    selected = nselected;
                }
                hi = true;
            }

            if (!hi && ((GetComponent<Encontrolmentation>().flags & 8UL) == 8UL || (DoubleTime.UnscaledTimeRunning - z.Item1 > 0.7f && (z.Item2 & 8UL) == 8UL && a == 0)))
            {
                t = (float)DoubleTime.ScaledTimeSinceLoad;
                a = Mathf.FloorToInt(Mathf.Max(25f - 4f * ((float)(DoubleTime.UnscaledTimeRunning - z.Item1)), 0f));
                int nselected = Mathf.Min(selected + 1, selections.Count - 1);
                if (nselected == selected)
                {
                    selected = 0;

                }
                else
                {
                    selected = nselected;
                }
                hi = true;
            }

            if (!hi && ((GetComponent<Encontrolmentation>().flags & 16UL) == 16UL))
            {
                if (selected == 3)
                {
                    Application.OpenURL("http://www.example.com");
                }
                else if (additiveNextScenes[selected])
                {
                    SceneManager.LoadScene(nextScenes[selected], LoadSceneMode.Additive);
                    GetComponent<Encontrolmentation>().currentState = 0;
                    GetComponent<Encontrolmentation>().allowUserInput = false;
                }
                else
                {
                    SceneManager.LoadScene(nextScenes[selected], LoadSceneMode.Single);
                }
            }

            }
	}
}
