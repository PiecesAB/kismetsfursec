using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Clock1 : MonoBehaviour
{
    public static DateTime now = DateTime.Now;
    public int myID = -1;
    public static List<Clock1> all = new List<Clock1>();

    public Transform hour;
    public Transform minute;

    int currID = -1;

    /*public static void UpdateClock()
    {
        now = DateTime.Now;
    }*/

    void Start()
    {
        currID++;
        myID = currID;
        all.Add(this);
    }

    void OnDestroy()
    {
        currID--;
        all.RemoveAt(myID);
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].myID > myID)
            {
                all[i].myID--;
            }
        }
    }

    void Update()
    {
        if (myID == 0)
        {
            now = DateTime.Now;
        }

        if (hour)
        {
            hour.localEulerAngles = Vector3.back * ( (now.Hour * 30f) + (now.Minute * 0.5f) );
        }

        if (minute)
        {
            minute.localEulerAngles = Vector3.back * ((now.Minute * 6f) + (now.Second * 0.1f));
        }
    }
}
