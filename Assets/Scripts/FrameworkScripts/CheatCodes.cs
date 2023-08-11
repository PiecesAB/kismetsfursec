using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class CheatCodes {

    // How convenient! Looks like all the codes were stored here. But I already hacked the game, so maybe I'd rather create my own cheats.
    public static string[] cheatNames =
    {
        "+100 million score. OK.",
        "+1 million score. Is that a lot?",
        "+3.5 million score. Are you enjoying this?",
        "\"hid<!>e<!>en test\" \n It's spelled H-I-D-D-E-N T-E-S-T. There now you know.",
        "\"hi<!>i<!>den test\" \n But you find yourself less stupid than that!",
        "\"hidden <!>tset<!>\" \n Did you fail your latest spelling tset?",
        "\"<!>hiden<!> test\" \n How many fingers do you have?",
        "Infinite jump! Don't go any place stupid.",
        "No collision for the rest of the level. Enjoy your trivial game...",
        "All tails in the level are paralyzed. Is it even possible anymore?",
    };

    public static int hiddenActivated = 0;
    public static bool misspelledHiddenTest = false;
    //Make sure no cheat codes contain another
    public static void Actions(int i)
    {
        if (i == 0) //+100m Code
        {
            if (hiddenActivated >= 10)
            {
                cheatNames[0] = "+100 million in-game score.";
            }
            else if(hiddenActivated >= 9)
            {
                cheatNames[0] = "Fine, just be nobody forever. It's not like this game really cares.";
            }
            else if(hiddenActivated >= 8)
            {
                cheatNames[0] = "Go cheat real life and be completely blissful! Never work again! Buy anything you want!";
            }
            else if (hiddenActivated >= 7)
            {
                cheatNames[0] = "Use them to skip past your entire life!";
            }
            else if (hiddenActivated >= 6)
            {
                cheatNames[0] = "Can you find the cheat codes to reality?";
            }
            else if (hiddenActivated >= 5)
            {
                cheatNames[0] = "Chances are, you just found this code on some wiki.";
            }
            else if (hiddenActivated >= 4)
            {
                cheatNames[0] = "Does this make you feel accomplished?";
            }
            else if (hiddenActivated >= 3)
            {
                cheatNames[0] = "So you're gonna cheat to get the best stats?";
            }
            else if (hiddenActivated >= 2)
            {
                cheatNames[0] = "+100 million score... again...";
            }
            else if (misspelledHiddenTest)
            {
                misspelledHiddenTest = false;
                cheatNames[0] = "There you go! +100 million score.";
            }
            Utilities.ChangeScore(100000000);
            hiddenActivated++;
        }
        if (i == 1) //+1m Code
        {
            Utilities.ChangeScore(1000000);
        }
        if (i == 2)
        {
            Utilities.ChangeScore(3500000);
        }
        if (i >= 3 && i <= 6)
        {
            misspelledHiddenTest = true;
        }
        if (i == 7)
        {
            BasicMove[] allPlrBMs = Object.FindObjectsOfType<BasicMove>();
            for (int j = 0; j < allPlrBMs.Length; ++j)
            {
                allPlrBMs[j].youCanJump = true;
                allPlrBMs[j].youCanDoubleJump = true;
                allPlrBMs[j].youCanInfinityJump = true;
            }
        }
        if (i == 8)
        {
            BasicMove[] allPlrBMs = Object.FindObjectsOfType<BasicMove>();
            for (int j = 0; j < allPlrBMs.Length; ++j)
            {
                allPlrBMs[j].TurnOffCollisionForever();
            }
        }
        if (i == 9)
        {
            BasicMove[] allPlrBMs = Object.FindObjectsOfType<BasicMove>();
            for (int j = 0; j < allPlrBMs.Length; ++j)
            {
                allPlrBMs[j].youCanDoubleJump = false;
            }
        }
    }

    public static string[] cheatsB =
    {
        "hidden test",
        "small loan",
        "5 sigma",
        "hideen test",
        "hiiden test",
        "hidden tset",
        "hiden test",
        "kangaroo",
        "journalist mode",
        "no2jump",
    };

    private static bool init = false;

    private static string currCode = "";

    public static void Init()
    {
        if (Keyboard.current == null || init) { return; }
        Keyboard.current.onTextInput += (c) =>
        {
            if (!LevelInfoContainer.main) { return; }
            int code = c;
            if (c >= 'A' && c <= 'Z') { c = c.ToString().ToLower()[0]; } // just in case
            if (code == 13) {
                for (int i = 0; i < cheatsB.Length; ++i)
                {
                    if (currCode.Length >= cheatsB[i].Length && currCode.Substring(currCode.Length - cheatsB[i].Length) == cheatsB[i])
                    {
                        Actions(i);
                        Utilities.lastCheatCode = cheatNames[i];
                        break;
                    }
                }
                currCode = "";
            }
            else
            {
                currCode += c;
                if (currCode.Length > 25) { currCode = currCode.Substring(currCode.Length - 25); } 
            }
        };
        init = true;
    }
}
