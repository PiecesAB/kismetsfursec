using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MrYeast : MonoBehaviour, IChoiceUIResponse
{
    public Transform eyes;

    private Vector3 eyesInitPos;
    private Vector3 origScale;

    public GameObject ChoiceResponse(string text)
    {
        GiveMoney(text);
        return null;
    }

    private void Start()
    {
        if ((Utilities.loadedSaveData.score >= 1000000 || Utilities.loadedSaveData.deathsThisLevel < 6) && !Application.isEditor)
        {
            Destroy(gameObject);
            return;
        }

        eyesInitPos = eyes.position;
        origScale = transform.localScale;
    }

    private void Update()
    {
        if (transform.localScale == origScale)
        {
            float closestMag = 250000f;
            Transform closestObj = null;
            for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; ++i)
            {
                if (LevelInfoContainer.allPlayersInLevel[i] == null) { continue; }
                Transform plrt = LevelInfoContainer.allPlayersInLevel[i].transform;
                float pmag = (plrt.position - eyesInitPos).sqrMagnitude;
                if (pmag < closestMag)
                {
                    closestObj = plrt;
                    closestMag = pmag;
                }
            }

            if (closestObj)
            {
                eyes.position = Vector3.MoveTowards(eyesInitPos, closestObj.position, 4f);
            }
        }
    }

    public void GiveMoney(string text)
    {
        if (text == "yes")
        {
            Utilities.ChangeScore(1000000);
        }
    }
}
