using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PrimBezierRender))]
[ExecuteInEditMode]
public class ReflectAxis : MonoBehaviour, IExaminableAction
{
    public bool activate;
    public float remainingRotation;
    [Header("P0 is transform position; P1 is other point in local position")]
    public Vector3 p1;
    public Transform objToReflect;
    public Transform rotateIcon;
    [Range(0f,1f)]
    public float rotateIconPosition;

    public BoxCollider2D myCollider;

    private Quaternion initRot = Quaternion.identity;
    private Quaternion finalRot = Quaternion.identity;
    private Vector3 initPos = Vector3.zero;
    private Vector3 finalPos = Vector3.zero;

    void Start()
    {
        RenderMe();
    }

    public void OnExamine(Encontrolmentation plr)
    {
        activate = true;
    }

    void RenderMe()
    {
        var pbr = GetComponent<PrimBezierRender>();
        pbr.bezier.points = new List<Bezier.Point>()
        {
            new Bezier.Point(Vector3.zero),
            new Bezier.Point(p1),
        };
        pbr.bezier.loop = false;
        pbr.checkMeToRender = true;

        rotateIcon.localPosition = p1*rotateIconPosition;
        float angle = Mathf.Atan2(p1.y, p1.x);
        rotateIcon.localEulerAngles = new Vector3(0f, 0f, Mathf.Repeat(angle * Mathf.Rad2Deg, 180f));

        myCollider.transform.localPosition = p1 * 0.5f;
        myCollider.size = new Vector2(p1.magnitude, 14f);
        myCollider.transform.eulerAngles = new Vector3(0f, 0f, angle * Mathf.Rad2Deg);
    }

    Vector3 ClosestAxisPoint(Vector3 p)
    {
        Vector3 pa1 = p1.normalized;
        Vector3 centered = p - transform.position;
        float dis = Vector3.Dot(pa1, centered);
        return (pa1 * dis) + transform.position;
    }

    void Update()
    {
        

        if (Time.timeScale > 0 && Application.isPlaying)
        {
            if (remainingRotation == 0f && activate && 
                (!objToReflect.GetComponent<primExtraTags>() || !objToReflect.GetComponent<primExtraTags>().tags.Contains("being reflected"))
                )
            {
                remainingRotation = 180f;
                initRot = finalRot = objToReflect.rotation;
                finalRot *= Quaternion.AngleAxis(180f, p1.normalized);
                initPos = objToReflect.position;
                Vector3 a1 = ClosestAxisPoint(objToReflect.position);
                Vector3 a2 = a1 - objToReflect.position;
                finalPos = a2+a1;
                activate = false;
                rotateIcon.gameObject.SetActive(false);

                primExtraTags pet0 = objToReflect.GetComponent<primExtraTags>();
                if (!pet0)
                {
                    pet0 = objToReflect.gameObject.AddComponent<primExtraTags>();
                    pet0.tags = new List<string>();
                }
                pet0.tags.Add("being reflected");

                foreach (Transform c in objToReflect)
                {
                    if (c.GetComponent<Collider2D>() && !c.GetComponent<PrimCollectibleAux>())
                    {
                        c.GetComponent<Collider2D>().enabled = false;
                    }
                    if (c.GetComponentInChildren<BasicMove>())
                    {
                        c.GetComponentInChildren<BasicMove>().Unparent();
                    }
                    if (c.GetComponent<SpriteRenderer>())
                    {
                        c.GetComponent<SpriteRenderer>().color = new Color(0.7f,0.7f,0.7f,0.7f);
                    }
                }
            }

            if (remainingRotation > 0f)
            {
                activate = false;
                if (remainingRotation > 3f)
                {
                    objToReflect.rotation = initRot;
                    objToReflect.rotation *= Quaternion.AngleAxis(180f - remainingRotation, p1.normalized);
                    objToReflect.position = Vector3.Lerp(initPos, finalPos, (Mathf.Cos(remainingRotation * Mathf.Deg2Rad)+1f)*0.5f);
                    remainingRotation -= 3f;
                }
                else
                {
                    objToReflect.rotation = finalRot;
                    objToReflect.position = finalPos;
                    remainingRotation = 0f;

                    rotateIcon.gameObject.SetActive(true);
                    foreach (Transform c in objToReflect)
                    {
                        if (c.GetComponent<Collider2D>())
                        {
                            c.GetComponent<Collider2D>().enabled = true;
                        }
                        if (c.GetComponent<SpriteRenderer>())
                        {
                            c.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        c.localPosition = new Vector3(c.localPosition.x, c.localPosition.y, c.localPosition.z);
                    }

                    primExtraTags pet = objToReflect.GetComponent<primExtraTags>();
                    while (pet.tags.Contains("being reflected"))
                    {
                        pet.tags.Remove("being reflected");
                    }
                }
            }

        }

        if (Application.isEditor && !Application.isPlaying)
        {
            RenderMe();
        }
    }
}
