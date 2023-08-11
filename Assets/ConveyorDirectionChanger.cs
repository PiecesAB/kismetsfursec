using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorDirectionChanger : MonoBehaviour
{
    public Vector2 changeTime;

    private ConveyorHelp[] allChilds;

    private Coroutine mainLoop;

    void Start()
    {
        allChilds = new ConveyorHelp[transform.childCount];
        for (int i = 0; i < transform.childCount; ++i)
        {
            allChilds[i] = transform.GetChild(i).GetComponent<ConveyorHelp>();
        }
        mainLoop = StartCoroutine(MainLoop());
    }

    private void OnDestroy()
    {
        StopCoroutine(mainLoop);
    }

    IEnumerator MainLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Fakerand.Single(changeTime.x, changeTime.y));
            for (int i = 0; i < allChilds.Length; ++i)
            {
                if (allChilds[i] != null) { allChilds[i].speed = -allChilds[i].speed; }
            }
        }
    }

    void Update()
    {
        
    }
}
