using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchChanger : MonoBehaviour, IExaminableAction {

    public Sprite onSprite;
    public Sprite offSprite;
    [Range(0, 31)]
    public int switchID;
    public bool on;
    private uint actualID;
    private double timeStuf;
    [SerializeField]
    private Door1 makeDoorNontrivial;
    [SerializeField]
    private AudioSource changeSound;

    public static HashSet<SwitchChanger> all = new HashSet<SwitchChanger>();

    public void Changed(uint nmask)
    {
        if ((actualID & nmask) != 0)
        {
            on = true;
            GetComponent<SpriteRenderer>().sprite = onSprite;

        }
        else
        {
            on = false;
            GetComponent<SpriteRenderer>().sprite = offSprite;

        }
    }

    // Use this for initialization
    void Awake()
    {
        actualID = 1u << switchID;
        timeStuf = 0;
        all.Add(this);
    }

    void OnDestroy()
    {
        all.Remove(this);
    }

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        actualID = 1u << switchID;
        timeStuf = 0;
        sr.material.SetColor("_RepColor", Utilities.colorCycle[switchID]);
        if ((actualID & Utilities.loadedSaveData.switchMask) != 0)
        {
            on = true;
            sr.sprite = onSprite;

        }
        else
        {
            on = false;
            sr.sprite = offSprite;
        }
    }

    public void OnExamine(Encontrolmentation plr)
    {
        Activate();
    }

    public void Activate()
    {
        timeStuf = DoubleTime.UnscaledTimeRunning;
        changeSound.Stop(); changeSound.Play();
        Utilities.ChangeSwitchRequest(actualID);
        if (makeDoorNontrivial)
        {
            makeDoorNontrivial.trivialCompletion = false;
        }
    }
}
