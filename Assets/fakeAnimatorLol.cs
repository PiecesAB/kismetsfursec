using UnityEngine;
using System.Collections;

public class fakeAnimatorLol : MonoBehaviour
{

    public Sprite one;
    public Sprite two;
    public Sprite three;
    private bool db = false;
    private float timeNow = 0.2f;
    // Use this for initialization
    void Start()
    {
    }
    

    // Update is called once per frame
    void Update()
    {
        db = false;
        if (DoubleTime.ScaledTimeSinceLoad > timeNow)
        {
            timeNow = timeNow + 0.2f;
            if (GetComponent<CertainFrameBlock>().on == false)
            {
                
                if (GetComponent<SpriteRenderer>().sprite.name == one.name && !db)
                {
                    GetComponent<SpriteRenderer>().sprite = two;
                    db = true;
                }
               
                if (GetComponent<SpriteRenderer>().sprite.name == two.name && !db)
                {
                    GetComponent<SpriteRenderer>().sprite = three;
                    db = true;
                }
               
                if (GetComponent<SpriteRenderer>().sprite.name == three.name && !db)
                {
                    GetComponent<SpriteRenderer>().sprite = one;
                    db = true;
                }
            }
        }
    }
}
