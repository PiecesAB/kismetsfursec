using UnityEngine;
using System.Collections;

public class GoodMouseTestButton : MonoBehaviour {

    public int score;
    private int oldScore;
    public Vector3 target1;
    public GameObject prefabMouseFloaty;
    public GameObject prefabMouseFollow;
    private Vector3 center;
    private float t =0 ;
    private float accel;
    
	// Use this for initialization
	void Start () {
        score = 0;
        oldScore = -1;
        accel = 0;
        center = transform.position;
        target1 = new Vector3(Mathf.Clamp(center.x + Random.Range(-4 * score, 4 * score), -120, 136), Mathf.Clamp(center.y + Random.Range(-4 * score, 4 * score), -280, -104), 0);
    }
	
    void OnMouseDown()
    {
        score++;
    }



	// Update is called once per frame
	void Update () {
        /*Vector3 v3Pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        v3Pos = Camera.main.ScreenToWorldPoint(v3Pos);

        if (score > 6)
        {
            if (!(oldScore == score))
            {
                transform.position = new Vector3(Mathf.Clamp(center.x+Random.Range(-6 * score, 6 * score),-120,136), Mathf.Clamp(center.y + Random.Range(-6 * score, 6 * score), -280, -104), 0);
            }
        }

        if (score > 11)
        {
            float mag = (target1 - transform.position).magnitude;
            //print(Vector3.MoveTowards(transform.position, target1, accel));
            //print(transform.position);
            transform.position = Vector3.MoveTowards(transform.position, target1, accel);
            if (mag > 48 )
            {
                accel = Mathf.Clamp(accel + 0.08f, 0, 3f);
            }
            if (mag <= 48)
            {
                accel = Mathf.Clamp(accel -0.08f, 0, 3f);
            }
            if (mag < 15 || accel < 0.2f)
            {

                target1 = new Vector3(Mathf.Clamp(center.x + Random.Range(-4 * score, 4 * score), -120, 136), Mathf.Clamp(center.y + Random.Range(-4 * score, 4 * score), -280, -104), 0);
            }
        }


        if ((score >= 19 && score < 25))
        {
            if (!(oldScore == score))
            {
                Object hi =  Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
            }
        }

        if (score >= 26 && score < 31)
        {
            if (!(oldScore == score))
            {
                Object hi = Instantiate(prefabMouseFollow, v3Pos, Quaternion.identity);
            }
        }


        if (score == 32)
        {
            if (!(oldScore == score))
            {
                
                Object hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
               hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
                hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
                 hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
                 hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
               hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
                 hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
              hi = Instantiate(prefabMouseFloaty, v3Pos, Quaternion.identity);
            }
        }


        oldScore = score;*/
    }
}
