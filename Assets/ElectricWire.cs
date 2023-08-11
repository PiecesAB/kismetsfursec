using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricWire : MonoBehaviour
{
    // to use this hashtable search, we have to assume no wire ever moves!

    public static PositionHashtable<ElectricWire> allWires = new PositionHashtable<ElectricWire>(1024, (e) => { return e.transform.position; });
    public static ElectricWire main;

    [HideInInspector]
    public ElectricWire[] neighbors = { null, null, null, null };
    //{ right, up,  left, down }

    public static AmorphousTileDatabase tileData;
    public SpriteRenderer mainSprite;
    public SpriteRenderer secondSprite;
    public Mode mode;
    public OutDirection outDirection;

    [HideInInspector]
    public bool thisIsOn = false;
    
    public enum OutDirection
    {
        Right, Up, Left, Down, None
    }

    public enum Mode
    {
        Normal, Resistor, Capacitor, And, Or, Not, Xor, Cross
    }

    private static Vector2[] neighborDirs = { Vector2.right * 16f, Vector2.up * 16f, Vector2.left * 16f, Vector2.down * 16f };

    private static OutDirection[] sameDirs = { OutDirection.Right, OutDirection.Up, OutDirection.Left, OutDirection.Down };
    private static OutDirection[] oppositeDirs = { OutDirection.Left, OutDirection.Down, OutDirection.Right, OutDirection.Up };

    private const float wireTime = 0.1f;
    private const float wireResistTime = 1f;
    private const float wireCapacitorTime = 1f;
    private const float stunIntensity = 0.25f;

    private int counter1 = 0;
    private bool logicOn;
    private bool oldLogicOn;

    private void Awake()
    {
        if (main == null) {
            main = this;
        }
        allWires.Add(this);

    }

    private void OnDestroy()
    {
        allWires.Remove(this);
        if (main == this) {
            allWires = new PositionHashtable<ElectricWire>(1024, (e) => { return e.transform.position; });
            main = null;
        }
    }

    private void ChangeShape()
    {
        if (mainSprite == null || !mainSprite.enabled) { return; }

        byte mask = 0;
        for (int i = 0; i < 4; ++i)
        {
            if (neighbors[i] != null) { mask += (byte)(1 << i); }
        }

        if (mode == Mode.Resistor)
        {
            switch (mask)
            {
                case 5: mainSprite.sprite = tileData.cross_UL[0]; break;
                case 10: mainSprite.sprite = tileData.cross_UR[0]; break;
                default: mainSprite.sprite = tileData.cross[0]; break;
            }
            return;
        }

        if (mode == Mode.Capacitor)
        {
            switch (mask)
            {
                case 5: mainSprite.sprite = tileData.cross_DL[0]; break;
                case 10: mainSprite.sprite = tileData.cross_DR[0]; break;
                default: mainSprite.sprite = tileData.cross[0]; break;
            }
            return;
        }

        switch (mask)
        {
            case 1: mainSprite.sprite = tileData.rightProtrusion[0]; break;
            case 2: mainSprite.sprite = tileData.stalactite[0]; break;
            case 3: mainSprite.sprite = tileData.topRightElbow[0]; break;
            case 4: mainSprite.sprite = tileData.leftProtrusion[0]; break;
            case 5: mainSprite.sprite = tileData.horizontal[0]; break;
            case 6: mainSprite.sprite = tileData.topLeftElbow[0]; break;
            case 7: mainSprite.sprite = tileData.TUp[0]; break;
            case 8: mainSprite.sprite = tileData.stalagmite[0]; break;
            case 9: mainSprite.sprite = tileData.bottomRightElbow[0]; break;
            case 10: mainSprite.sprite = tileData.vertical[0]; break;
            case 11: mainSprite.sprite = tileData.TRight[0]; break;
            case 12: mainSprite.sprite = tileData.bottomLeftElbow[0]; break;
            case 13: mainSprite.sprite = tileData.TDown[0]; break;
            case 14: mainSprite.sprite = tileData.TLeft[0]; break;
            case 15: case 0: default:
                if (mode == Mode.Cross) { mainSprite.sprite = tileData.vertical[0]; }
                else { mainSprite.sprite = tileData.cross[0]; }
                break;
        }

        if (secondSprite == null || !secondSprite.enabled) { return; }

        switch (mode)
        {
            case Mode.Not: secondSprite.sprite = tileData.floor[0]; break;
            case Mode.And: secondSprite.sprite = tileData.leftWall[0]; break;
            case Mode.Or: secondSprite.sprite = tileData.rightWall[0]; break;
            case Mode.Xor: secondSprite.sprite = tileData.ceiling[0]; break;
            case Mode.Cross: secondSprite.sprite = tileData.center[0]; break;
            default: break;
        }

        if (mode == Mode.Cross) { return; }

        switch (outDirection)
        {
            case OutDirection.Right: secondSprite.transform.eulerAngles = Vector3.zero; break;
            case OutDirection.Up: secondSprite.transform.eulerAngles = Vector3.forward * 90f; break;
            case OutDirection.Left: secondSprite.transform.eulerAngles = Vector3.forward * 180f; secondSprite.flipY = true; break;
            case OutDirection.Down: secondSprite.transform.eulerAngles = Vector3.forward * -90f; secondSprite.flipY = true; break;
            default: break;
        }

        
    }

    private void Start()
    {
        thisIsOn = false;
        logicOn = false;
        counter1 = 0;
        if (main == this)
        {
            tileData = GameObject.FindGameObjectWithTag("ElectricWireTileset").GetComponent<AmorphousTileDatabase>();
        }

        // find my four neighbors
        neighbors = new ElectricWire[4]{ null, null, null, null };
        for (int i = 0; i < 4; ++i)
        {
            neighbors[i] = allWires.Fetch((Vector2)transform.position + neighborDirs[i]);
        }

        //change tile
        ChangeShape();

        //set "not" gate
        if (mode == Mode.Not)
        {
            StartCoroutine(Off());
        }
    }

    public IEnumerator On(OutDirection sourceDir = OutDirection.None, bool negated = false)
    {
        if (mode == Mode.Not && !negated)
        {
            StartCoroutine(Off(sourceDir, true));
            yield break;
        }

        if (thisIsOn && !(mode == Mode.Cross || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)) { yield break; }
        if (mode == Mode.Cross)
        {
            int dirJunk = (sourceDir == OutDirection.Down || sourceDir == OutDirection.Up) ? 2 : 1;
            if ((counter1 & dirJunk) != 0f) { yield break; }
            counter1 |= dirJunk;
        }
        if ((mode == Mode.Not || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && sourceDir == outDirection) { yield break; }

        oldLogicOn = logicOn;
        if (mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)
        {
            ++counter1;
            switch (mode)
            {
                case Mode.And: logicOn = (counter1 >= 2); break;
                case Mode.Or: logicOn = (counter1 >= 1); break;
                case Mode.Xor: logicOn = (counter1 == 1); break;
                default: break;
            }
        }

        thisIsOn = true;

        if (mainSprite != null && mainSprite.enabled)
        {
            if (mode == Mode.Cross && secondSprite != null && secondSprite.enabled)
            {
                if ((counter1 & 1) == 1) { secondSprite.material.SetFloat("_Stun", stunIntensity); }
                else { secondSprite.material.SetFloat("_Stun", 0f); }
                if ((counter1 & 2) == 2) { mainSprite.material.SetFloat("_Stun", stunIntensity); }
                else { mainSprite.material.SetFloat("_Stun", 0f); }
            }
            else if ((mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && secondSprite != null && secondSprite.enabled)
            {
                if (logicOn)
                {
                    secondSprite.material.SetFloat("_Stun", stunIntensity * 0.5f);
                    mainSprite.material.SetFloat("_Stun", stunIntensity);
                }
                else
                {
                    secondSprite.material.SetFloat("_Stun", 0f);
                    mainSprite.material.SetFloat("_Stun", 0f);
                }
            }
            else
            {
                mainSprite.material.SetFloat("_Stun", stunIntensity);
            }
        }

        if (mode == Mode.Not && secondSprite != null && secondSprite.enabled) { secondSprite.material.SetFloat("_Stun", stunIntensity*0.5f); }

        if (mode == Mode.Resistor) { yield return new WaitForSeconds(wireResistTime); }
        else { yield return new WaitForSeconds(wireTime); }
        
        for (int i = 0; i < 4; ++i)
        {
            if (neighbors[i] == null) { continue; }
            if (sameDirs[i] == sourceDir) { continue; }
            if ((mode == Mode.Not || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && sameDirs[i] != outDirection) { continue; }
            if (mode == Mode.Cross && oppositeDirs[i] != sourceDir) { continue; }

            if (mode == Mode.Capacitor)
            {
                if (counter1 == 0)
                {
                    counter1 = 1;
                    StartCoroutine(neighbors[i].On(oppositeDirs[i]));
                    yield return new WaitForSeconds(wireCapacitorTime);
                    counter1 = 2;
                    StartCoroutine(neighbors[i].Off(oppositeDirs[i]));
                }
            }
            else if (mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)
            {
                if (logicOn && !oldLogicOn) { StartCoroutine(neighbors[i].On(oppositeDirs[i])); }
                if (!logicOn && oldLogicOn) { StartCoroutine(neighbors[i].Off(oppositeDirs[i])); }
            }
            else
            {
                StartCoroutine(neighbors[i].On(oppositeDirs[i]));
            }
            
        }
        yield return null;
    }

    public IEnumerator Off(OutDirection sourceDir = OutDirection.None, bool negated = false)
    {
        if (mode == Mode.Not && !negated)
        {
            StartCoroutine(On(sourceDir, true));
            yield break;
        }

        if (!thisIsOn && !(mode == Mode.Cross || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)) { yield break; }
        if (mode == Mode.Cross)
        {
            int dirJunk = (sourceDir == OutDirection.Down || sourceDir == OutDirection.Up) ? 2 : 1;
            if ((counter1 & dirJunk) == 0f) { yield break; }
            counter1 &= ~dirJunk;
        }
        if ((mode == Mode.Not || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && sourceDir == outDirection) { yield break; }

        oldLogicOn = logicOn;
        if (mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)
        {
            --counter1;
            switch (mode)
            {
                case Mode.And: logicOn = (counter1 >= 2); break;
                case Mode.Or: logicOn = (counter1 >= 1); break;
                case Mode.Xor: logicOn = (counter1 == 1); break;
                default: break;
            }
        }

        thisIsOn = false;
        if (mainSprite != null && mainSprite.enabled) {
            if (mode == Mode.Cross && secondSprite != null && secondSprite.enabled)
            {
                if ((counter1 & 1) == 1) { secondSprite.material.SetFloat("_Stun", stunIntensity); }
                else { secondSprite.material.SetFloat("_Stun", 0f); }
                if ((counter1 & 2) == 2) { mainSprite.material.SetFloat("_Stun", stunIntensity); }
                else { mainSprite.material.SetFloat("_Stun", 0f); }
            }
            else if ((mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && secondSprite != null && secondSprite.enabled)
            {
                if (logicOn)
                {
                    secondSprite.material.SetFloat("_Stun", stunIntensity * 0.5f);
                    mainSprite.material.SetFloat("_Stun", stunIntensity);
                }
                else
                {
                    secondSprite.material.SetFloat("_Stun", 0f);
                    mainSprite.material.SetFloat("_Stun", 0f);
                }
            }
            else
            {
                mainSprite.material.SetFloat("_Stun", 0f);
            }
        }

        if (mode == Mode.Not && secondSprite != null && secondSprite.enabled) { secondSprite.material.SetFloat("_Stun", 0f); }

        if (mode == Mode.Resistor) { yield return new WaitForSeconds(wireResistTime); }
        else { yield return new WaitForSeconds(wireTime); }

        for (int i = 0; i < 4; ++i)
        {
            if (neighbors[i] == null) { continue; }
            if (sameDirs[i] == sourceDir) { continue; }
            if ((mode == Mode.Not || mode == Mode.And || mode == Mode.Or || mode == Mode.Xor) && sameDirs[i] != outDirection) { continue; }
            if (mode == Mode.Cross && oppositeDirs[i] != sourceDir) { continue; }
            if (mode == Mode.Capacitor)
            {
                if (counter1 == 0)
                {
                    StartCoroutine(neighbors[i].Off(oppositeDirs[i]));
                }
                else if (counter1 == 2)
                {
                    counter1 = 3;
                    StartCoroutine(neighbors[i].On(oppositeDirs[i]));
                    yield return new WaitForSeconds(wireCapacitorTime);
                    counter1 = 0;
                    StartCoroutine(neighbors[i].Off(oppositeDirs[i]));
                }
            }
            else if (mode == Mode.And || mode == Mode.Or || mode == Mode.Xor)
            {
                if (logicOn && !oldLogicOn) { StartCoroutine(neighbors[i].On(oppositeDirs[i])); }
                if (!logicOn && oldLogicOn) { StartCoroutine(neighbors[i].Off(oppositeDirs[i])); }
            }
            else
            {
                StartCoroutine(neighbors[i].Off(oppositeDirs[i]));
            }
        }
        yield return null;
    }


}
