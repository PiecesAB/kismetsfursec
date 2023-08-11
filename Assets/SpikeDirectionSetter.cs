using UnityEngine;
using System.Collections;

public class SpikeDirectionSetter : MonoBehaviour {

    [Range(0, 360)]
    public int degreesOffset;
    [HideInInspector]
    public Vector2 directionToDie;
    public string deathReason = "generic spike";

	void Start () {
	    if (directionToDie == Vector2.zero)
        {
            float a = (transform.eulerAngles.z + degreesOffset) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            directionToDie = dir;
        }
	}

}
