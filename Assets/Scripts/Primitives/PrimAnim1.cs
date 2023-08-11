using UnityEngine;
using System.Collections;

public class PrimAnim1 : MonoBehaviour {

    public float selfOnTime;
    public float selfOffTime;
    public float selfDelay;
    //Add a rhythm tracker
    public string propertyName;
    public bool isBool;
    public bool currentBool;
    private float myTime;

	void Start () {
        myTime = 0f;
        selfOnTime = Mathf.Max(0.01666666f, selfOnTime);
        selfOffTime = Mathf.Max(0.01666666f, selfOffTime);

    }
	
	void Update () {

        if (isBool)
        {
            if (currentBool)
            {
                while (myTime + selfOnTime + selfDelay < DoubleTime.ScaledTimeSinceLoad)
                {
                    myTime += selfOnTime;
                    GetComponent<Animator>().SetBool(propertyName, false);
                    currentBool = false;
                    if (myTime + selfOnTime + selfDelay >= DoubleTime.ScaledTimeSinceLoad)
                    {
                        myTime -= selfOnTime;
                        myTime += selfOffTime;
                    }
                }
            }
            if (!currentBool)
            {
                while (myTime + selfOffTime + selfDelay < DoubleTime.ScaledTimeSinceLoad)
                {
                    myTime += selfOffTime;
                    GetComponent<Animator>().SetBool(propertyName, true);
                    currentBool = true;
                    if (myTime + selfOffTime + selfDelay >= DoubleTime.ScaledTimeSinceLoad)
                    {
                        myTime -= selfOffTime;
                        myTime += selfOnTime;
                    }
                }
            }
        }


    }
}
