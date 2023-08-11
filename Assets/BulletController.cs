using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BulletController
{
    private static readonly BulletControllerHelper helper;

    public static BulletControllerHelper GetHelper()
    {
        return helper;
    }

    public static void Load()
    {
        // this is weird.
        // but it makes sure that the static class is being used.
        // called from Encontrolmentation
    }

    static BulletController()
    {
        GameObject helperObj = new GameObject { name = "Danmaku Controller" };
        helper = helperObj.AddComponent<BulletControllerHelper>();
    }
}
