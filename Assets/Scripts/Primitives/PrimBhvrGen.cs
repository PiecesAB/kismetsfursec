using UnityEngine;
using System.Collections;

public class PrimBhvrGen : MonoBehaviour {

    [System.Serializable]
    public struct ObjectData
    {
        public GameObject genObject;
        public Vector3 position;
        public Vector3 rotation;
        //public Vector3 localscale;
    }

    public float startDelay = 0f;

    public ObjectData[] stuffToMake;
    public float[] waitIntervals;
    public float changeInRotation;
    public bool useRelativePositionToThisObject;

    public int genIndexStart;
    public int waiterIndexStart;
    public bool waiterIndexLoop = true;
    public float destroyTime;

    private Transform camTrans = null;
    public float activeRadius = Mathf.Infinity;


    public IEnumerator Loop()
    {
        if (startDelay > 0f) { yield return new WaitForSeconds(startDelay); }

        while (true)
        {
            if (camTrans == null) { camTrans = Camera.main.transform; }
            if ((transform.position - camTrans.position).magnitude <= activeRadius)
            {
                Vector3 vt = new Vector3(stuffToMake[genIndexStart].rotation.x, stuffToMake[genIndexStart].rotation.y, (float)(stuffToMake[genIndexStart].rotation.z + ((changeInRotation * DoubleTime.ScaledTimeSinceLoad) % 360)));
                Vector3 vp = stuffToMake[genIndexStart].position;
                if (useRelativePositionToThisObject) { vp = transform.TransformPoint(vp); }
                GameObject newObj = (GameObject)Instantiate(stuffToMake[genIndexStart].genObject, vp, Quaternion.Euler(vt));
                newObj.SetActive(true);
                //newObj.transform.localScale = stuffToMake[genIndexStart].localscale;
                Destroy(newObj, destroyTime);
            }
            double ct = DoubleTime.ScaledTimeSinceLoad;
            while (ct + waitIntervals[waiterIndexStart] > DoubleTime.ScaledTimeSinceLoad)
            {
                yield return 1;
            }
            genIndexStart = (int)Mathf.Repeat(genIndexStart + 1, stuffToMake.Length);
            if (waiterIndexLoop)
            {
                waiterIndexStart = (int)Mathf.Repeat(waiterIndexStart + 1, waitIntervals.Length);
            }
            else if (waiterIndexStart < waitIntervals.Length - 1)
            {
                ++waiterIndexStart;
            }
        }
    }

	void Start () {
        camTrans = Camera.main.transform;
        StartCoroutine(Loop());
	}
	
	void Update () {

	}
}
