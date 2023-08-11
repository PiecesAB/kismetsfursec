using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialSetter : DialBase
{
    [SerializeField]
    private GameObject[] connected;

    protected override void ChildStart()
    {
        UpdateConnected();
    }

    private void UpdateConnected()
    {
        for (int i = 0; i < connected.Length; ++i)
        {
            GameObject g = connected[i];

            Component[] comps = g.GetComponents(typeof(IDialSetter));
            for (int c = 0; c < comps.Length; ++c)
            {
                ((IDialSetter)comps[c]).DialChanged(myValue);
            }
        }
    }

    protected override void ChildUpdate()
    {
        if (dx != 0)
        {
            UpdateConnected();
        }
    }
}
