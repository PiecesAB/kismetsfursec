using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewWireNode1 : MonoBehaviour {

    public List<Transform> toMove;
    public Transform nextNode;

    private TestScriptRotateScene tsrs;

    public float speed;

    // Use this for initialization
    void Start () {
        GetComponentInChildren<LineRenderer>().enabled = true;
        GetComponentInChildren<LineRenderer>().useWorldSpace = true;
        /* foreach (GameObject i in FindObjectsOfType<GameObject>())
        {
            if (i.transform.position == transform.position && toMove == null && i.transform.parent != transform && i.transform.parent != nextNode.transform)
            {
                toMove = i.transform;
                working = true;
            }
        } */
        tsrs = FindObjectOfType<TestScriptRotateScene>();
	}

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            if (!tsrs || (tsrs && tsrs.remainingRotation == 0f))
            {

                Vector3[] lol = { transform.position, nextNode.position };
                List<Transform> clean = new List<Transform>();
                GetComponentInChildren<LineRenderer>().SetPositions(lol);
                foreach (Transform m in toMove)
                {
                    if (m != null)
                    {
                        Vector3 lastPos = m.position;
                        m.position = Vector3.MoveTowards(m.position, nextNode.position, speed * Time.timeScale);
                        m.position = Vector3.Lerp(transform.position, nextNode.position, (m.position - transform.position).magnitude / (nextNode.position - transform.position).magnitude);
                        //print((m.position - lastPos).magnitude);
                        if (m.GetComponent<Rigidbody2D>())
                        {
                            m.GetComponent<Rigidbody2D>().MovePosition(m.position);
                        }


                        if ((m.position - nextNode.position).magnitude < 0.00001f/*< speed * Time.timeScale*/)
                        {

                            m.position = nextNode.position;
                            NewWireNode1 n = nextNode.gameObject.GetComponentInChildren<NewWireNode1>();
                            n.toMove.Add(m);
                            clean.Add(m);
                        }

                        
                    }
                    /*if (m == null)
                    {
                        toMove.Remove(m);
                    }*/
                    
                }

                foreach (Transform m in clean)
                {
                    toMove.Remove(m);
                }
            }
        }
    }
}
