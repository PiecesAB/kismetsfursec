using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideRouletteFunction : MonoBehaviour, IChoiceUIResponse
{
    private string[] colors = new string[38]
    {
        "green", "green",
        "red", "black", "red", "black", "red", "black", "red", "black", "red", "black",
        "black", "red", "black", "red", "black", "red", "black", "red",
        "red", "black", "red", "black", "red", "black", "red", "black", "red", "black",
        "black", "red", "black", "red", "black", "red", "black", "red",
    };

    public GameObject winBox;
    public GameObject loseBox;

    public GameObject ChoiceResponse(string text)
    {
        return Fire(text);
    }

    /*private void GiveBox(GameObject vbox)
    {
        GameObject ne = Instantiate(vbox, Vector3.zero, Quaternion.identity);
        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
        ne.SetActive(true);
    }*/

    private string NumStr(int n)
    {
        if (n > 0) { return (n-1).ToString(); }
        return "00";
    }

    private int ForcedWin(string s)
    {
        if (s == "green") { return Fakerand.Int(0, 2); }
        int n = Fakerand.Int(2, 38);
        while (colors[n] != s) { n = Fakerand.Int(2, 38); }
        return n;
    }

    public GameObject Fire(string choice)
    {
        int randChoice = Fakerand.Int(0, 38);
        bool win = (colors[randChoice] == choice);
        print(win);

        float extraChance = Utilities.loadedSaveData.score / 50000000f;
        if (Fakerand.Single() < extraChance) { win = true; randChoice = ForcedWin(choice); }

        MainTextsStuff.insertableStringValue1 = "" + NumStr(randChoice) + " " + colors[randChoice];

        if (!win)
        {
           // GiveBox(loseBox);
            return loseBox;
        }

        int randScore = Fakerand.Int(500000, 1000001);
        if (randChoice < 1) { randScore *= 18; }
        MainTextsStuff.insertableIntValue1 = randScore;
        Utilities.ChangeScore(randScore);
        //GiveBox(winBox);
        return winBox;
    }
}
