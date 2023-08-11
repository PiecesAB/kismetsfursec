﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportMachine : MonoBehaviour,IDialSetter {

    public enum sor
    {
        Sender,Reciever
    }
    
    public bool on;
    public sor type;
    [Range(0,99)]
    public int ID;
    public GameObject nachbar;
    public TextMesh textmesh;
    private bool runn = false;
    public float barTime;
    public AudioSource inSound;
    public AudioSource outSound;

    private int oldID;

    private Color txtOrigCol;

    private List<TeleportMachine> targets = new List<TeleportMachine>();
    private static Vector3 outOfBoundsZone = new Vector3(9999999f, 9999999f, 0f);

    private static HashSet<TeleportMachine> all = new HashSet<TeleportMachine>();

    public void DialChanged(float val)
    {
        ID = Mathf.RoundToInt(val);
    }

    IEnumerator TeleMan(Vector2 lol, Vector2 fvel, TeleportMachine tt, BasicMove bm)
    {
        FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
        if (tt == null) { yield break; }
        Vector3 newPos = tt.transform.position - 1f * (Vector3)lol - (Vector3.up * 3f);
        ftp.target = null;
        bm.transform.position = outOfBoundsZone;
        bm.gameObject.SetActive(false);

        BulletRegister.ClearNonScrolling();

        barTime = 20f;
        if (inSound)
        {
            inSound.Stop();
            inSound.Play();
        }
        yield return new WaitUntil(delegate () { return (barTime == -20f) && (Time.timeScale > 0); });

        Renderer ttr = tt.GetComponent<Renderer>();

        if (ftp.perScreenScrolling)
        {
            ftp.MoveTowardsByScreen(tt.transform.position);
        }
        else if (ftp.originalScrolling)
        {
            ftp.target = tt.transform;
            yield return new WaitUntil(delegate () { return (ftp.autoGeneratedVelocity.sqrMagnitude < 0.01f) && ttr.isVisible && (Time.timeScale > 0); });
        }

        tt.barTime = 20f;
        if (outSound)
        {
            outSound.Stop();
            outSound.Play();
        }
        yield return new WaitUntil(delegate () { return tt.barTime == -20f; });
        //yield return new WaitForSeconds(0.5f*Time.timeScale);
        yield return new WaitUntil(delegate () { return Time.timeScale > 0; });
        bm.gameObject.SetActive(true);
        bm.transform.position = newPos;
        bm.fakePhysicsVel = fvel;
        //if (ftp.originalScrolling)
        {
            ftp.target = bm.transform;
        }
    }

    void UpdateVisual()
    {
        GetComponent<SpriteRenderer>().material.SetColor("_RepColor", Utilities.colorCycle[ID % 32]);
        if (textmesh != null)
        {
            textmesh.text = "" + ID;
            txtOrigCol = Color.Lerp(Utilities.colorCycle[ID % 32], Color.white, 0.5f);
            textmesh.color = txtOrigCol;
        }
    }

    void MakeTargets()
    {
        targets.Clear();
        foreach (var i in all)
        {
            if (i.type == sor.Reciever && i.ID == ID)
            {
                targets.Add(i);
            }
        }
    }

	void Start () {
        all.Add(this);
        barTime = -20f;
        UpdateVisual();
	/*if (on && type == sor.Sender)
        {
            
        }*/
	}

    void OnDestroy()
    {
        all.Remove(this);
    }

    private void TrigCheck(Collider2D other)
    {
        MakeTargets();
        if (type == sor.Sender && targets.Count > 0 && runn)
        {
            BasicMove bm = other.gameObject.GetComponent<BasicMove>();
            FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
            if (bm && bm.CanCollide && ftp.target == bm.transform && (other.transform.position - transform.position).magnitude <= 40)
            {
                Vector2 lol = transform.InverseTransformPoint(other.transform.position);
                Vector2 vel = other.gameObject.GetComponent<Rigidbody2D>().velocity;
                TeleportMachine tt = targets[Fakerand.Int(0, targets.Count)];
                if (GetComponent<TeleportMachinePuzzle1>())
                {
                    GetComponent<TeleportMachinePuzzle1>().MoveDown();
                }
                StartCoroutine(TeleMan(lol, vel, tt, bm));

            }
            //teleport other things soon
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TrigCheck(other);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TrigCheck(other);
    }

    void Update() {
        runn = true;
        barTime = Mathf.Max(barTime - 1.5f * Time.timeScale, -20f);
        nachbar.transform.localPosition = Vector3.up * barTime;
        nachbar.transform.localScale = new Vector3(nachbar.transform.localScale.x, 0.06f - (0.003f * System.Math.Abs(barTime)), nachbar.transform.localScale.z);
        if (textmesh != null)
        {
            textmesh.color = Color.Lerp(Color.black, txtOrigCol, System.Math.Abs((float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * 5.3f)));
        }

        if (oldID != ID)
        {
            UpdateVisual();
            oldID = ID;
        }
    }
}
