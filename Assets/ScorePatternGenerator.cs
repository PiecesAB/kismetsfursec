using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePatternGenerator : MonoBehaviour
{
    
    public enum Mode
    {
        Circle
    }

    public Mode mode;
    public float r; //radius??
    public int num; //how many obj?
    public bool giveRotation;
    public float angleOffset;
    public float completion = 1f;
    public bool makeActiveOnStart;

    public GameObject scoreObj;

    private const float tau = 6.28318531f;

    void Start()
    {
        if (mode == Mode.Circle)
        {
            for (float w = 0; w < tau*completion - 0.00001f; w+= tau*completion/num)
            {
                GameObject n = Instantiate(scoreObj, transform);
                if (makeActiveOnStart)
                {
                    n.SetActive(true);
                }
                n.transform.localPosition = r * new Vector3(Mathf.Cos(w), Mathf.Sin(w));
                n.transform.localRotation = Quaternion.AngleAxis((giveRotation?(w*Mathf.Rad2Deg):0f) + angleOffset,Vector3.forward);
            }
        }
    }

    
    void Update()
    {
        
    }
}
