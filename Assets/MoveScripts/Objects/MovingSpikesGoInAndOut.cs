using UnityEngine;
using System.Collections;

public class MovingSpikesGoInAndOut : MonoBehaviour {

    public float StartDelay;
    public float InDelay;
    public float OutDelay;
    public float movingWaitTime;
    public bool StartOut;
    private float oldTime = 0;
    private bool db = false;

	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().sortingLayerName = "BG";
	}
	
    IEnumerator Out()
    {
        db = true;
                yield return new WaitForSeconds(InDelay);
                for (int i = 0; i < 8; i++)
                {
                    transform.position = transform.position + new Vector3(2*GetComponent<SpikeDirectionSetter>().directionToDie.x, 2*GetComponent<SpikeDirectionSetter>().directionToDie.y, 0);
                    yield return new WaitForSeconds(movingWaitTime);
                }
        db = false;
    }

    IEnumerator In()
    {
        db = true;
        yield return new WaitForSeconds(InDelay);
        for (int i = 0; i < 8; i++)
        {
            transform.position = transform.position - new Vector3(2 * GetComponent<SpikeDirectionSetter>().directionToDie.x, 2 * GetComponent<SpikeDirectionSetter>().directionToDie.y, 0);
            yield return new WaitForSeconds(movingWaitTime);
        }
        db = false;
    }

   /* IEnumerator Wait(float timelol)
    {
        yield return new WaitForSeconds(timelol);
    } */

	// Update is called once per frame
	void Update () {
	
        if (oldTime == 0f && !db)
        {
            if (DoubleTime.ScaledTimeSinceLoad >= StartDelay && !db)
            {
                if (StartOut == false && !db)
                {
                    oldTime = StartDelay + (movingWaitTime * 8) + OutDelay;
                    StartOut = true;
                    db = true;
                    StartCoroutine(Out());
                }

                if (StartOut == true && !db)
                {
                    oldTime = StartDelay + (movingWaitTime * 8) + InDelay;
                    StartOut = false;
                    db = true;
                    StartCoroutine(In());
                }
            }
        }

        if (oldTime != 0f && DoubleTime.ScaledTimeSinceLoad >= oldTime && !db)
        {
            
                if (StartOut == false && !db)
                {
                    oldTime = oldTime + (movingWaitTime*8) + OutDelay;
                StartOut = true;
                db = true;
                StartCoroutine(Out());
                }

                if (StartOut == true && !db)
                {
                oldTime = oldTime + (movingWaitTime * 8) + InDelay;
                StartOut = false;
                db = true;
                StartCoroutine(In());
                }
          
        }



    }
}
