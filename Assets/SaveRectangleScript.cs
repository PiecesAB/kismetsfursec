using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[System.Obsolete("Save Rectangle is for primitive save screen and no longer needed.")]
public class SaveRectangleScript : MonoBehaviour {

    //public GameObject saveBelow;
    //public GameObject saveAbove;
    //public byte saveNumber;
    //public bool initialized;
    //public byte whichButtonSelected;
    //public bool testingInitNew;

    //public Utilities.SaveData saveCorrespondent;

    /*public float HPCalc(float num)
    {
        float c1 = Mathf.Pow(10, 0.2f) * 15;
        float newSize = Mathf.Pow(num / c1, 2.5f) * (3 / 5);
        return newSize;
    }


    public void AllChildrenStuff(Transform t)
    {
        foreach (Transform child in t)
        {
            bool overrideActive = false;
            GameObject g = child.gameObject;

            if (g.name == "TimePlayed")
            {
                byte seconds = (byte)(saveCorrespondent.gameTimePlayed % 60);
                byte minutes = (byte)((Mathf.Floor((float)saveCorrespondent.gameTimePlayed / 60))%60);
                int hours = (int)(Mathf.Floor((float)saveCorrespondent.gameTimePlayed / 3600));
                string fmt = " 00";
                g.GetComponent<Text>().text = hours+":"+ minutes.ToString(fmt).Substring(1) + ":"+seconds.ToString(fmt).Substring(1);
            }
            if (g.name == "ChapterName")
            {
                g.GetComponent<Text>().text = "Chp:" + saveCorrespondent.chapter;
               
            }
            if (g.name == "DifficultyName")
            {
                g.GetComponent<Text>().text = "Dif:" + saveCorrespondent.difficulty;
            }



            if (g.name == "Button1Text")
            {
                g.GetComponent<Text>().text = "DEL";
            }
            if (g.name == "Button2Text")
            {
                g.GetComponent<Text>().text = "OPEN";
            }


            if (g.name == "KHealth")
            {
                
                g.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(HPCalc(saveCorrespondent.SharedPlayersHP), 4);
            }
            if (g.name == "THealth")
            {
                if (saveCorrespondent.numberOfPlayersUnlocked >= 2)
                {
                    g.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(0f, 4);
                }
                else
                {
                    overrideActive = true;
                }
            }
            if (g.name == "MHealth")
            {
                if (saveCorrespondent.numberOfPlayersUnlocked >= 3)
                {
                    g.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(0f, 4);
                }
                else
                {
                    overrideActive = true;
                }
            }
            if (g.name == "?Health")
            {
                if (saveCorrespondent.numberOfPlayersUnlocked == 4)
                {
                    g.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(0f, 4);
                }
                else
                {
                    overrideActive = true;
                }
            }
            if (g.name == "RoomName")
            {
                g.GetComponent<Text>().text = saveCorrespondent.levelName;
                g.GetComponent<Text>().color = new Color(0.85f, 0.85f, 0.85f, 1);
            }


            if (!overrideActive)
            {
                g.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
            AllChildrenStuff(child);
        }
    }

    public void AllChildrenStuff2(Transform t)
    {
        foreach (Transform child in t)
        {
            GameObject g = child.gameObject;
            if (g.name == "Button1Text")
            {
                g.GetComponent<Text>().text = "INIT";
            }
            else if(g.name == "RoomName")
            {
                g.GetComponent<Text>().text = "Nonexistant game.";
                g.GetComponent<Text>().color = new Color(0.75f, 0, 0, 1);
            }
            else if (g.name == "Button1")
            {
                //sorry nothing
            }
            else
            {
                g.SetActive(false);
            }


            AllChildrenStuff2(child);
        }
    }

    public void RedrawStats(Utilities.SaveData data)
    {
        AllChildrenStuff(transform);
    }

    public void ShowEmptyStats()
    {
        AllChildrenStuff2(transform);
    }

    // Use this for initialization
    void Start () {
        if (testingInitNew)
        {
            Utilities.InitializeGame(saveNumber);
        }

            saveCorrespondent = Utilities.LoadGame(saveNumber);


        if (saveCorrespondent != null)
        {
            initialized = true;
            RedrawStats(saveCorrespondent);
        }
        else
        {
            initialized = false;
            ShowEmptyStats();
        }
	}
	
	*/
}
