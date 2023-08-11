using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMachinePuzzle1 : MonoBehaviour
{
    [SerializeField]
    private Transform moveDownObject;
    [SerializeField]
    private TeleportMachine lowMachine;
    [SerializeField]
    private TeleportMachine highMachine;
    [SerializeField]
    private TeleportMachine correctMachine;

    [SerializeField]
    private BigNumberDisplay numDisp;

    [SerializeField]
    private int correctID;

    private int oldMainID;

    public int triesLeft;


    private TeleportMachine mainMachine;

    private IEnumerator MoveDownCoroutine()
    {
        --triesLeft;
        numDisp.number = triesLeft;
        int i = 0;
        while (i < 8 || triesLeft <= 0)
        {
            if (Time.timeScale > 0)
            {
                moveDownObject.position += Vector3.down;
                ++i;
            }
            yield return new WaitForEndOfFrame();
        }
        yield break;
    }

    public void MoveDown()
    {
        StartCoroutine(MoveDownCoroutine());
    }

    private void Start()
    {
        mainMachine = GetComponent<TeleportMachine>();
        correctID = Fakerand.Int(0, 100);
        triesLeft = 9;
        oldMainID = mainMachine.ID;
        numDisp.number = triesLeft;
    }

    private void Update()
    {
        if (oldMainID != mainMachine.ID)
        {
            if (Fakerand.Single() < Utilities.loadedSaveData.score / 1e8f)
            {
                correctID = mainMachine.ID;
            }
            oldMainID = mainMachine.ID;
        }

        if (mainMachine.ID < correctID)
        {
            lowMachine.ID = mainMachine.ID;
            highMachine.ID = correctMachine.ID = (mainMachine.ID + 1) % 100;
        }
        else if (mainMachine.ID > correctID)
        {
            lowMachine.ID = correctMachine.ID = (mainMachine.ID + 99) % 100;
            highMachine.ID = mainMachine.ID;
        }
        else
        {
            lowMachine.ID = (mainMachine.ID + 1) % 100;
            correctMachine.ID = mainMachine.ID;
            highMachine.ID = (mainMachine.ID + 99) % 100;
        }
    }
}
