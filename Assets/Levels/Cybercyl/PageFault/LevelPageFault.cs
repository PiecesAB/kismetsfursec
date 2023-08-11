using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPageFault : MonoBehaviour
{
    public float totalLoadTime = 2.5f;
    public AnimationCurve loadingBarCurve;
    public GameObject ui;
    public RectTransform loadingBar;
    public Text loadingTextObject;
    public string loadingText = "loading danmaku...";
    public SpecialGate[] gates;
    public float gatesOpenDelay = 3f;
    public LevelPageFault nextInSequence;
    public float nextInSequenceDelay = 1f;

    private bool debounce = false;

    private IEnumerator LoadBullets()
    {
        Encontrolmentation plrE = LevelInfoContainer.GetActiveControl();
        if (!plrE) { throw new System.Exception("You suck!"); }
        Vector2 plrPos = plrE.transform.position;
        if (debounce) { yield break; }
        debounce = true;
        float origTS = Time.timeScale;
        Utilities.canPauseGame = false;
        Utilities.canUseInventory = false;
        Time.timeScale = 0;
        ui.SetActive(true);
        loadingTextObject.text = loadingText;

        double startTime = DoubleTime.UnscaledTimeSinceLoad;
        Transform shooterHolder = transform.GetChild(0);
        BulletHellMakerFunctions[] shooters = new BulletHellMakerFunctions[shooterHolder.childCount];
        for (int i = 0; i < shooterHolder.childCount; ++i)
        {
            shooters[i] = shooterHolder.GetChild(i).GetComponent<BulletHellMakerFunctions>();
        }

        int currShooter = 0;
        int currPos = 0;
        Transform positionHolder = transform.GetChild(1);
        int totalShots = positionHolder.childCount;

        while (DoubleTime.UnscaledTimeSinceLoad - startTime < totalLoadTime)
        {
            float rprog = (float)(DoubleTime.UnscaledTimeSinceLoad - startTime) / totalLoadTime;
            float aprog = loadingBarCurve.Evaluate(rprog);
            loadingBar.sizeDelta = new Vector2(aprog * 100f, loadingBar.sizeDelta.y);
            yield return new WaitForEndOfFrame();

            while (currPos < totalShots && (float)(currPos + 1) / totalShots <= aprog)
            {
                Vector3 pos = positionHolder.GetChild(currPos).position;
                if ((((Vector2)pos) - plrPos).magnitude >= 32)
                {
                    shooters[currShooter].transform.position = pos;
                    shooters[currShooter].transform.rotation = positionHolder.GetChild(currPos).rotation;
                    shooters[currShooter].FireWhilePaused();
                }
                currShooter = (currShooter + 1) % shooters.Length;
                ++currPos;
            }
        }

        while (currPos < totalShots)
        {
            shooters[currShooter].transform.position = positionHolder.GetChild(currPos).position;
            shooters[currShooter].transform.rotation = positionHolder.GetChild(currPos).rotation;
            shooters[currShooter].Fire();
            currShooter = (currShooter + 1) % shooters.Length;
            ++currPos;
        }

        ui.SetActive(false);
        Time.timeScale = origTS;
        Utilities.canPauseGame = true;
        Utilities.canUseInventory = true;
        yield return null;

        if (nextInSequence)
        {
            yield return new WaitForSeconds(nextInSequenceDelay);
            nextInSequence.StartCoroutine(nextInSequence.LoadBullets());
        }

        if (gatesOpenDelay > 0f)
        {
            yield return new WaitForSeconds(gatesOpenDelay);
        }

        foreach (SpecialGate gate in gates)
        {
            gate.target = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        Destroy(GetComponent<Collider2D>());
        StartCoroutine(LoadBullets());
    }

    private void Start()
    {
        Transform positionHolder = transform.GetChild(1);
        for (int i = 0; i < positionHolder.childCount; ++i)
        {
            positionHolder.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
