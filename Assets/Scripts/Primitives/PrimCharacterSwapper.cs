using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimCharacterSwapper : MonoBehaviour
{
    private SpecialGunTemplate gun;
    private Encontrolmentation encmt;
    private FollowThePlayer ftp;

    public static Sprite youSprite = null;
    public static GameObject swapEffect = null;
    private SpriteRenderer youDisp;

    public static int cooldown = 0;

    private void ShowYouSign()
    {
        if (LevelInfoContainer.playableCount < 2) { return; }
        GameObject newSignObj = new GameObject("YOU", new System.Type[] { typeof(SpriteRenderer) });
        Transform newTrans = newSignObj.transform;
        newTrans.SetParent(transform);
        newTrans.localPosition = new Vector3(0, 24, 0);
        SpriteRenderer newSR = newTrans.GetComponent<SpriteRenderer>();
        newSR.material = GetComponent<SpriteRenderer>().material;
        newSR.sortingLayerName = "Right Above Player";
        newSR.sprite = youSprite;
        youDisp = newSR;
    }

    void Start()
    {
        if (youSprite == null) { youSprite = Resources.Load<Sprite>("YOU"); }
        if (swapEffect == null) { swapEffect = Resources.Load<GameObject>("PlayerSwapEffect"); }
        gun = GetComponent<SpecialGunTemplate>();
        if (cooldown > 0)
        {
            gun.swapCooldownDisabled = true;
        }
        encmt = GetComponent<Encontrolmentation>();
        if (ftp == null) { ftp = LevelInfoContainer.mainFtp; }
        ShowYouSign();
        PlayerExclusionBlock.UpdatePlayerName(encmt.GetComponent<PrimPlayableCharacter>());
    }

    private int GetMyIndex()
    {
        for (int i = 0; i < LevelInfoContainer.allCtsInLevel.Length; ++i)
        {
            if (LevelInfoContainer.allCtsInLevel[i] == encmt)
            {
                return i;
            }
        }
        return -1;
    }

    private void DoTheSwitch(Encontrolmentation nextEncmt)
    {
        if (youDisp) { Destroy(youDisp.gameObject); youDisp = null; }

        nextEncmt.allowUserInput = true;
        encmt.allowUserInput = false;
        PrimCharacterSwapper pcs = nextEncmt.gameObject.AddComponent<PrimCharacterSwapper>();
        pcs.ftp = ftp;

        // add swap effect
        GameObject nswap = Instantiate(swapEffect, transform.position, Quaternion.identity, null);
        PlayerSwapEffect pse = nswap.GetComponent<PlayerSwapEffect>();
        pse.target = nextEncmt.transform;

        ftp.target = nextEncmt.transform;
        ftp.refPlayer = nextEncmt.gameObject;
        if (ftp.perScreenScrolling) {
            // try to use markers to get the scroll position, if possible
            int succCode;
            Vector3 p = cameraScrModifierObject.PSSGetCameraPosForPoint(nextEncmt.transform.position, out succCode);
            if (succCode >= 0)
            {
                ftp.SetTransformPosition(new Vector3(p.x, p.y, ftp.transform.position.z));
                ftp.stun += 2;
            }
            else
            {
                ftp.MoveTowardsByScreen(nextEncmt.transform.position);
            }
        }
        CharacterSquaresUI.Trigger();
        cooldown = 2;

        Destroy(this);
    }

    public void ForceSwitch(Encontrolmentation other)
    {
        DoTheSwitch(other);
    }

    private void AttemptSwitch(int dir)
    {
        if (LevelInfoContainer.playableCount < 2) { return; }
        int myIndex = GetMyIndex();
        if (myIndex == -1) { return; } // When does this ever happen?
        int len = LevelInfoContainer.allCtsInLevel.Length;
        int nextIndex = (myIndex + dir + len) % len;
        while (LevelInfoContainer.allCtsInLevel[nextIndex] == null) { nextIndex = (nextIndex + dir + len) % len; }
        DoTheSwitch(LevelInfoContainer.allCtsInLevel[nextIndex]);
    }

    private bool AllIsWell()
    {
        return Time.timeScale > 0 && !KHealth.someoneDied && !Door1.levelComplete;
    }
    
    void Update()
    {
        if (cooldown > 0) {
            --cooldown;
            if (cooldown == 0)
            {
                gun.swapCooldownDisabled = false;
            }
        }
        else
        {
            if (gun && encmt && !gun.isAiming && encmt.allowUserInput && AllIsWell())
            {
                if ((encmt.flags & 768UL) == 256UL) { AttemptSwitch(-1); }
                else if ((encmt.flags & 768UL) == 512UL) { AttemptSwitch(1); }
            }

            if (youDisp && youDisp.isVisible)
            {
                Color c = youDisp.color;
                if (c.a > 0) { youDisp.color = new Color(c.r, c.g, c.b, c.a - 0.02f); }
                else { Destroy(youDisp.gameObject); youDisp = null; }
            }
        }
    }
}
