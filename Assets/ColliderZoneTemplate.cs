using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColliderZoneTemplate<T> : MonoBehaviour
{

    public static List<Collider2D> collidedThisFrame = new List<Collider2D>();
    public static List<Collider2D> otherColliders = new List<Collider2D>();

    public static int IDMaker = -1;

    public static List<ColliderZoneTemplate<T>> allTDZ = new List<ColliderZoneTemplate<T>>();

    public int myID = -1;

    public abstract void ResetStuff();

    void Start()
    {
        IDMaker++;
        if (IDMaker == 0)
        {
            collidedThisFrame.Clear();
            otherColliders.Clear();
            ResetStuff();
            
        }
        myID = IDMaker;
        allTDZ.Add(this);
        SendMessage("ExtraStart", SendMessageOptions.DontRequireReceiver);
    }

    private void OnDestroy()
    {
        IDMaker--;
        for (int i = 0; i < allTDZ.Count; i++)
        {
            if (allTDZ[i].myID > myID)
            {
                allTDZ[i].myID--;
            }
        }
    }

    public abstract void ColliderAdd();

    void OnTriggerEnter2D(Collider2D c)
    {
        Rigidbody2D r2 = c.GetComponent<Rigidbody2D>();
        BasicMove bmCheck = c.GetComponent<BasicMove>();
        if (r2 && !r2.isKinematic /*&& (!bmCheck || bmCheck.CanCollide)*/ && !(c is EdgeCollider2D))
        {
            collidedThisFrame.Add(c); // allow duplicates
            otherColliders.Add(GetComponent<Collider2D>());
            ColliderAdd();
        }
    }

    public abstract void ColliderRemove(int index);

    private void RemoveTheEntry(int i)
    {
        collidedThisFrame.RemoveAt(i);
        otherColliders.RemoveAt(i);
        ColliderRemove(i);
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        int i = collidedThisFrame.IndexOf(c);
        if (i != -1) { RemoveTheEntry(i); }
    }

    public abstract void ObjectIn(int index, GameObject obj, GameObject other);

    void Update()
    {
        if (myID != 0) { return; }

        HashSet<int> gamObjJunk = new HashSet<int>();
        for (int i = 0; i < collidedThisFrame.Count; i++)
        {
            if (collidedThisFrame[i])
            {
                //colliders not overlapping? no collision!
                if (otherColliders[i])
                {
                    if (!otherColliders[i].IsTouching(collidedThisFrame[i]))
                    {
                        RemoveTheEntry(i); --i; continue;
                    }
                }

                int id = collidedThisFrame[i].gameObject.GetInstanceID();
                BasicMove bmCheck = collidedThisFrame[i].GetComponent<BasicMove>();
                if (!collidedThisFrame[i].gameObject.activeSelf /*|| (bmCheck && !bmCheck.CanCollide)*/)
                {
                    RemoveTheEntry(i); --i; continue;
                }
                else if (!gamObjJunk.Contains(id))
                {
                    if (otherColliders[i])
                    {
                        gamObjJunk.Add(id);
                        ObjectIn(i, collidedThisFrame[i].gameObject, otherColliders[i].gameObject);
                    }
                    else
                    {
                        RemoveTheEntry(i); --i; continue;
                    }
                }
            }
            else
            {
                collidedThisFrame.RemoveAt(i); i--;
            }
        }
    }
}
