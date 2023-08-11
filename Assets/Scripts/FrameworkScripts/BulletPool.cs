using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BulletPool
{
    public static GameObject advancedBulletObj;
    public static Queue<GameObject> storage = new Queue<GameObject>();

    private static int maxStorage = 2048;

    private static BulletPoolHelper helper;

    public static void Push(GameObject newObj)
    {
        if (storage.Count < maxStorage)
        {
            newObj.transform.SetParent(null);
            Object.DontDestroyOnLoad(newObj);
            newObj.transform.SetParent(helper.transform);
            storage.Enqueue(newObj);
            newObj.SetActive(false);
        }
        else //sorry, queue's full
        {
            Object.Destroy(newObj);
        }
    }

    public static GameObject Pop()
    {
        if (storage.Count == 0) { return Object.Instantiate(advancedBulletObj); }

        while (storage.Count > 0 && storage.Peek() == null) { storage.Dequeue(); }
        if (storage.Count == 0) { return Object.Instantiate(advancedBulletObj); }

        GameObject ret = storage.Dequeue();
        ret.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(ret, SceneManager.GetActiveScene());
        ret.SetActive(true);

        return ret;
    }

    static BulletPool()
    {
        advancedBulletObj = Resources.Load<GameObject>("Bullets/BulletWithAdvancedControl");
        GameObject helperObj = new GameObject();
        helperObj.name = "Danmaku Pooling Helper";
        helper = helperObj.AddComponent<BulletPoolHelper>();
    }
}
