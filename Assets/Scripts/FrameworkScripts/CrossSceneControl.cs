using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CrossSceneControl
{
    private static CrossSceneControlHelper helper;

    public static CrossSceneControlHelper GetHelper()
    {
        return helper;
    }

    public static void Load()
    {
        // static constructor
    }

    static CrossSceneControl()
    {
        GameObject helperObj = new GameObject();
        helperObj.name = "Cross-Scene Input Handler";
        helper = helperObj.AddComponent<CrossSceneControlHelper>();
    }
}
