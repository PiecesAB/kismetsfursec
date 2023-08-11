using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToSwapPositions : SpecialGunTemplate
{
    public GameObject uiHolder;
    public Transform uiArrows;
    
    public Sprite[] icons;

    public SpriteRenderer[] uiIcons = new SpriteRenderer[2];

    private int[] currChoices = new int[2];

    private int currSide;
    private Encontrolmentation[] plrList = null;

    private string[] names = new string[] { "Khal", "Tetra", "Myst", "Ravel", "Aktaion" };
    private int[] iconIds = new int[] { 1, 2, 3, 4, 5 };

    private List<BasicMove> ghosts = new List<BasicMove>();

    [SerializeField]
    private PermutohedronPuzzle specialPuzzle;

    private void CalculateAngle()
    {
        Encontrolmentation other = plrList[currChoices[currSide]];
        if (other == e)
        {
            nextangle = 0f;
            return;
        }
        Vector2 dir = transform.InverseTransformPoint(other.transform.position);
        nextangle = Fastmath.FastAtan2(dir.y, dir.x)*Mathf.Rad2Deg;
    }

    protected override void ChildStart()
    {
        currSide = 0;
    }

    protected override void AimingBegin()
    {
        if (plrList == null) { plrList = LevelInfoContainer.GetNonNullPlayerList(); }
        if (plrList.Length == 1) { currChoices = new int[] { 0, 0 }; }
        else { currChoices = new int[] { 0, 1 }; }
    }

    private void MoveChoices(int inc)
    {
        currChoices[currSide] += inc;
        if (currChoices[currSide] < 0) { currChoices[currSide] += plrList.Length; }
        if (currChoices[currSide] >= plrList.Length) { currChoices[currSide] -= plrList.Length; }
    }

    protected override void AimingUpdate()
    {
        if (e.ButtonDown(256UL, 768UL) || e.ButtonDown(512UL, 768UL)) { currSide = (currSide + 1) % 2; }
        if (e.ButtonDown(4UL, 12UL)) { MoveChoices(1); }
        if (e.ButtonDown(8UL, 12UL)) { MoveChoices(-1); }
        CalculateAngle();
    }

    private int GetIconId(Encontrolmentation plr)
    {
        PrimPlayableCharacter ppc = plr.GetComponent<PrimPlayableCharacter>();
        if (ppc == null || ppc.name == "") { return 0; }
        for (int i = 0; i < names.Length; ++i)
        {
            if (names[i] == ppc.myName) { return iconIds[i]; }
        }
        return 0;
    }

    private Sprite GetIcon(int side)
    {
        return icons[GetIconId(plrList[currChoices[side]])];
    }

    protected override float Fire()
    {
        Encontrolmentation a = plrList[currChoices[0]];
        Encontrolmentation b = plrList[currChoices[1]];

        if (a == b)
        {
            BasicMove bm = a.GetComponent<BasicMove>();
            if (ghosts.Contains(bm)) { return 1f; }
            ghosts.Add(bm);
            bm.swimming = true;
            print("ghost mode incomplete");
            return 1f;
        }

        Vector3 oldAPos = a.transform.position;
        Vector3 oldBPos = b.transform.position;
        PrimCharacterSwapper swapA = a.GetComponent<PrimCharacterSwapper>();
        PrimCharacterSwapper swapB = b.GetComponent<PrimCharacterSwapper>();

        a.transform.position = oldBPos;
        b.transform.position = oldAPos;
        if (swapA) { swapA.ForceSwitch(b); }
        else if (swapB) { swapB.ForceSwitch(a); }

        if (specialPuzzle)
        {
            List<string> plrNames = new List<string>{ "T", "K", "M", "R" };
            //assuming all players in this puzzle level are playable.
            int plrA = plrNames.IndexOf(a.GetComponent<PrimPlayableCharacter>().myName.Substring(0, 1)) + 1;
            int plrB = plrNames.IndexOf(b.GetComponent<PrimPlayableCharacter>().myName.Substring(0, 1)) + 1;
            if (plrA == 0 || plrB == 0) { throw new System.Exception(); }
            specialPuzzle.Swap(plrA, plrB);
        }

        return 1f;
    }

    protected override void GraphicsUpdateWhenAiming()
    {
        uiHolder.SetActive(true);
        uiArrows.localPosition = new Vector3(-12 + 24*currSide, 0, 0);

        uiIcons[0].sprite = GetIcon(0);
        uiIcons[1].sprite = GetIcon(1);
    }

    protected override void GraphicsUpdateWhenNotAiming()
    {
        uiHolder.SetActive(false);
    }

    protected override void ChildUpdate()
    {
        for (int i = 0; i < ghosts.Count; ++i)
        {
            BasicMove bi = ghosts[i];
            bi.swimCount = 3;
            bi.swimming = true;
            bi.ghost = true;
            bi.TurnOffCollisionForever();
            bi.fakePhysicsVel = new Vector2(0.9f * bi.fakePhysicsVel.x, 0.9f * bi.fakePhysicsVel.y);
        }
    }
}
