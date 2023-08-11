using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Linq;

public class FourThirtyAM : MonoBehaviour
{

    public Transform allLava;
    public Collider2D mainCollider;
    private bool open;
    private bool tested;
    private string username;

    void Start()
    {
        open = false;
        tested = false;
        username = Environment.UserName;
    }

    void Open()
    {
        mainCollider.enabled = false;
        foreach (Transform l in allLava)
        {
            l.GetComponent<BoostArrow>().isLava = false;
        }
    }

    void Close()
    {
        mainCollider.enabled = true;
        foreach (Transform l in allLava)
        {
            l.GetComponent<BoostArrow>().isLava = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!tested && col.GetComponent<BasicMove>())
        {
            tested = true;
            if (SpecialCheck()) { print("cheater");
                Utilities.LoadGame(Utilities.currentSaveNumber, false, true, 
                new string[] {
                    "Looks like you changed your system time manually.",
                    "That's not very fair. You're not a time traveller in real life.",
                    "So this game doesn't have to be fair either.",
                    "Your save file is now wiped from this computer, and a backup copy has been placed in memory.",
                    "If you want to see your data again, you must remain in this level for <!>three hours.",
                    "If you die or close the window, your save is gone forever!",
                    "And nope, changing the system time won't work here.",
                    "I'll see you back in the real game.",
                });
            }
            else { print("passed"); }
        }
    }



    bool SpecialCheck()
    {

        try
        {
            EventLog sysLog = new EventLog("System");
            if (sysLog == null) { return false; }
            for (int i = sysLog.Entries.Count - 50; i < sysLog.Entries.Count; ++i)
            {
                EventLogEntry e = sysLog.Entries[i];
                if (e.UserName == Environment.UserDomainName + "\\" + Environment.UserName
                    && e.Message == "The start type of the Windows Time service was changed from demand start to disabled.")
                {
                    //user very likely changed the local time
                    return true;
                }
            }
            return false;
        }
        catch (Exception e)
        {
            print(e);
        }


        return false;
    }

    void Update()
    {
        DateTime now = DateTime.Now;
        if (!open )//&& now.Hour == 4 && now.Minute == 30)
        {
            open = true;
            Open();
        }

        /*if (open && (now.Hour != 4 || now.Minute != 30))
        {
            open = false;
            Close();
        }*/
    }
}
