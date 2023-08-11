using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackTable : MonoBehaviour, ITextBoxDeactivate
{

    public enum WhoseCard
    {
        Player, Dealer
    }

    public enum EndState
    {
        NotYetDefined, Win, Loze, Tie, CreditCard, Joker
    }

    public int gameProgress;
    public RectTransform hitStandBox;
    public RectTransform dealersTurnBox;
    public List<int> plrCards = new List<int>();
    public double plrScore;
    public Text plrScoreDisplay;
    public List<int> dlrCards = new List<int>();
    public double dlrScore;
    public Text dlrScoreDisplay;
    public Sprite[] cardPics;
    public double[] cardValues;
    public int whereWeirdCardsStart;
    public Vector2 smallCardsRange;
    public Vector2 bigCardsRange;
    public Vector2 negCardsRange;
    public Vector2 jokeCardsRange;
    public Vector2 indexCardsRange;
    public Transform cardFromTable;
    public GameObject pcardInUISample;
    public GameObject dcardInUISample;
    public Material indexCard;
    public Material regularCard;
    public bool nowDrawingCard;
    public EndState endState;
    public RectTransform plrHandBox;
    public RectTransform pCardCntnr;
    public RectTransform dCardCntnr;
    public RectTransform dlrHandBox;
    public Transform importantUIs;
    public GameObject[] endStateBoxes;
    public bool thereWasAJoker;
    public Animator chipMan;
    public AudioSource cardDrawSound;
    public AudioSource awaitChoiceSound;
    Vector3 plrHandBoxOrigPos;
    Vector3 dlrHandBoxOrigPos;
    Vector3 cardOrigPos;
    Vector3 pScoreDispOrigPos;
    Vector3 dScoreDispOrigPos;
    Quaternion cardOrigRot;


    double CalcTotal(List<int> l)
    {
        int aces = 0;
        double total = 0;
        foreach (int i in l)
        {
            if (i != -1)
            {
                double ii = cardValues[i];
                if (ii == -1000)
                {
                    aces += 1;
                    total += 11;
                }
                else if (ii == -2000)
                {
                    aces += 1;
                    total -= 1;
                }
                else if (ii == -6000)
                {
                    return 21;
                }
                else if (ii > -1000)
                {
                    total += ii;
                }
            }
        }

        while (aces > 0 && total > 21)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    int PickOfAllCards()
    {
        switch (Fakerand.Int(0,12))
        {
            case 0:
                return Fakerand.Int((int)smallCardsRange.x, (int)smallCardsRange.y);
            case 1:
                return Fakerand.Int((int)bigCardsRange.x, (int)bigCardsRange.y);
            case 2:
                return Fakerand.Int((int)negCardsRange.x, (int)negCardsRange.y);
            case 3:
                return Fakerand.Int((int)jokeCardsRange.x, (int)jokeCardsRange.y);
            case 4:
                return Fakerand.Int((int)indexCardsRange.x, (int)indexCardsRange.y);
            default:
                return Fakerand.Int(0, whereWeirdCardsStart);
        }
    }

    IEnumerator Resign()
    {
        yield return new WaitForSeconds(2f);
        chipMan.SetTrigger("Resign");
    }

    IEnumerator EndClap()
    {
        yield return new WaitForSeconds(2f);
        chipMan.SetTrigger("Normal");
    }


    IEnumerator Liftoff()
    {
        float prog = 0f;
        while (prog < 1f)
        {
            Vector3 toPHBPos = plrHandBoxOrigPos + new Vector3(0, -100, 0);
            Vector3 toDHBPos = dlrHandBoxOrigPos + new Vector3(0, 100, 0);
            float tween = EasingOfAccess.CubicOut(prog);
            plrHandBox.localPosition = Vector3.Lerp(plrHandBoxOrigPos, toPHBPos, tween);
            dlrHandBox.localPosition = Vector3.Lerp(dlrHandBoxOrigPos, toDHBPos, tween);
            prog += 0.0333333333333f;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DrawCard(int amount, WhoseCard getMan, bool hidden, bool allCards, bool debounce = true)
    {
        if (debounce)
        {
            nowDrawingCard = true;
        }
        int amt = 0;
        while (amt < amount)
        {
            cardDrawSound.Play();
            float prog = 0f;
            Text ttm = dlrScoreDisplay;
            if (getMan == WhoseCard.Player)
            {
               ttm = plrScoreDisplay;
            }
            RectTransform ttmr = ttm.GetComponent<RectTransform>();
            Vector3 ttmp = ttmr.localPosition;
            Vector3 ttmpGoal = ttmp;
            if ((getMan == WhoseCard.Player && plrCards.Count < 7) || (getMan == WhoseCard.Dealer && dlrCards.Count < 7))
            {
                ttmpGoal = ttmp + new Vector3(25, 0, 0);
            }
            int cardVal = allCards ? PickOfAllCards() : Fakerand.Int(0, whereWeirdCardsStart);
            if (cardVal >= indexCardsRange.x && cardVal < indexCardsRange.y)
            {
                cardFromTable.GetComponent<Renderer>().material = indexCard;
            }
            Vector3 goal = (getMan == WhoseCard.Player) ? new Vector3(0, -0.7f, -2.5f) : new Vector3(0, 0, -2.5f);
            Quaternion goalRot = Quaternion.identity;
            if (!hidden)
            {
                goalRot = Quaternion.Euler(0, -90, 0);
            }

            while (prog < 1f)
            {

                float es = EasingOfAccess.SineSmooth(prog / 2f);
                cardFromTable.position = Vector3.Lerp(cardOrigPos, goal, es *2f);
                cardFromTable.rotation = Quaternion.Lerp(cardOrigRot, goalRot, prog);
                ttmr.localPosition = Vector3.Lerp(ttmp, ttmpGoal, es);
                prog += 0.0833333f;
                yield return new WaitForEndOfFrame();
            }
            
            cardFromTable.position = cardOrigPos;
            cardFromTable.rotation = cardOrigRot;
            cardFromTable.GetComponent<Renderer>().material = regularCard;

            ttmr.localPosition = Vector3.Lerp(ttmp, ttmpGoal, 0.5f);
            prog = 0f;

            if (getMan == WhoseCard.Player)
            {
                GameObject newCard = Instantiate(pcardInUISample);
                RectTransform rt = newCard.GetComponent<RectTransform>();
                Transform container = pCardCntnr;
                rt.SetParent(container, false);

                bool moveContainer = (plrCards.Count >= 7);
                Vector3 oldContainPos = container.localPosition;
                Vector3 newContainPos = (moveContainer) ? (container.localPosition - new Vector3(25, 0, 0)) : container.localPosition;

                if (!hidden)
                {
                    plrCards.Add(cardVal);
                    newCard.GetComponent<Image>().sprite = cardPics[cardVal];
                }
                else
                {
                    plrCards.Add(-1);
                }

                rt.localScale = Vector3.one;
                Vector3 localOrig = new Vector3(25* (Mathf.Max(plrCards.Count -1, 7)-7), 32, 0);
                rt.localPosition = localOrig;
                rt.rotation = goalRot;
                Quaternion fromRot = rt.rotation;
                Vector3 goalP2 = new Vector3(-135 + 25 * (plrCards.Count - 1), -6);
                while (prog < 1f)
                {
                    float es = EasingOfAccess.SineSmooth(0.5f + (prog / 2f));
                    rt.localPosition = Vector3.Lerp(localOrig, goalP2, 2f * (es - 0.5f));
                    rt.rotation = Quaternion.Lerp(fromRot, Quaternion.identity, prog);
                    ttmr.localPosition = Vector3.Lerp(ttmp, ttmpGoal, es);
                    container.localPosition = Vector3.Lerp(oldContainPos, newContainPos, EasingOfAccess.SineSmooth(prog));
                    prog += 0.0833333f;
                    yield return new WaitForEndOfFrame();
                }

                container.localPosition = newContainPos;
                rt.localPosition = goalP2;
                rt.rotation = Quaternion.identity;
            }
            else //Dealer
            {
                GameObject newCard = Instantiate(dcardInUISample);
                RectTransform rt = newCard.GetComponent<RectTransform>();
                Transform container = dCardCntnr;
                rt.SetParent(container, false);

                bool moveContainer = (dlrCards.Count >= 7);
                Vector3 oldContainPos = container.localPosition;
                Vector3 newContainPos = (moveContainer) ? (container.localPosition - new Vector3(25, 0, 0)) : container.localPosition;

                if (!hidden)
                {
                    dlrCards.Add(cardVal);
                    newCard.GetComponent<Image>().sprite = cardPics[cardVal];
                }
                else
                {
                    dlrCards.Add(-1);
                }

                rt.localScale = Vector3.one;
                Vector3 localOrig = new Vector3(25 * (Mathf.Max(dlrCards.Count - 1, 7) - 7), -56, 0);
                rt.localPosition = localOrig;
                rt.rotation = goalRot;
                Quaternion fromRot = rt.rotation;
                Vector3 goalP2 = new Vector3(-135 + 25 * (dlrCards.Count - 1), -6);
                while (prog < 1f)
                {
                    float es = EasingOfAccess.SineSmooth(0.5f + (prog / 2f));
                    rt.localPosition = Vector3.Lerp(localOrig, goalP2, 2f*(es-0.5f));
                    rt.rotation = Quaternion.Lerp(fromRot, Quaternion.identity, prog);
                    ttmr.localPosition = Vector3.Lerp(ttmp, ttmpGoal, es);
                    container.localPosition = Vector3.Lerp(oldContainPos, newContainPos, EasingOfAccess.SineSmooth(prog));
                    prog += 0.0833333f;
                    yield return new WaitForEndOfFrame();
                }
                container.localPosition = newContainPos;
                rt.localPosition = goalP2;
                rt.rotation = Quaternion.identity;
                
            }

            ttmr.localPosition = ttmpGoal;
            ttm.color = Color.white;
            if (cardValues[cardVal] == -3000) //joker
            {
                yield return new WaitForSeconds(0.3f);
                endState = EndState.Joker;
                if (!thereWasAJoker)
                {
                    chipMan.SetTrigger("Surprise");
                }
                else
                {
                    chipMan.SetTrigger("Angery");
                }
                yield return StartCoroutine(Liftoff());
                yield return new WaitForSeconds(0.7f);
                chipMan.SetTrigger("Normal");
                gameProgress = 8;
                if (!thereWasAJoker)
                {
                    GameObject msg = Instantiate(endStateBoxes[4]); //joker message
                    msg.transform.SetParent(importantUIs, true);
                    thereWasAJoker = true;
                    amt = 123456;
                }
                else
                {
                    GameObject msg = Instantiate(endStateBoxes[5]); //joker message
                    msg.transform.SetParent(importantUIs, true);
                    amt = 123456;
                    StartCoroutine(Resign());
                }
            }
            else if (getMan == WhoseCard.Player && cardValues[cardVal] == -5000) //credit card
            {
                yield return new WaitForSeconds(0.7f);
                yield return StartCoroutine(Liftoff());
                gameProgress = 8;
                endState = EndState.CreditCard;
                chipMan.SetTrigger("Thonk");
                int onCredit = MainTextsStuff.insertableIntValue1 = Mathf.RoundToInt(Fakerand.Int(100000, 401000) * Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier);
                Utilities.ChangeScore(Mathf.RoundToInt(onCredit));
                GameObject msg = Instantiate(endStateBoxes[3]); //credit card get
                msg.transform.SetParent(importantUIs, true);
                amt = 123456;
            }
            else
            {
                double cardTotal = CalcTotal((getMan == WhoseCard.Player) ? plrCards : dlrCards);
                if (getMan == WhoseCard.Player)
                {
                    plrScore = cardTotal;
                }
                else //Dealer
                {
                    dlrScore = cardTotal;
                }
                ttm.text = "= " + ((cardTotal <= 21) ? ("" + cardTotal) : "BUST");
                if (cardTotal > 21)
                {
                    ttm.color = new Color(1f, 0.6f, 0f, 1f);
                }
                else if (cardTotal >= 17)
                {
                    ttm.color = Color.yellow;
                }

                if (cardValues[cardVal] == -4000)
                {
                    yield return StartCoroutine(DrawCard(4, getMan, false, true, false));
                }
            }
            amt++;
        }
        if (debounce)
        {
            nowDrawingCard = false;
        }
    }

    void Start () {
        endState = EndState.NotYetDefined;
        plrScore = dlrScore = 0;
        nowDrawingCard = false;
        cardOrigPos = cardFromTable.position;
        cardOrigRot = cardFromTable.rotation;
        gameProgress = 0;
        hitStandBox.sizeDelta = dealersTurnBox.sizeDelta = new Vector2(0, 16);
        plrHandBoxOrigPos = plrHandBox.localPosition;
        dlrHandBoxOrigPos = dlrHandBox.localPosition;
        plrHandBox.localPosition = plrHandBoxOrigPos + new Vector3(0, -100, 0);
        dlrHandBox.localPosition = dlrHandBoxOrigPos + new Vector3(0, 100, 0);
        dScoreDispOrigPos = dlrScoreDisplay.GetComponent<RectTransform>().localPosition;
        pScoreDispOrigPos = plrScoreDisplay.GetComponent<RectTransform>().localPosition;
        thereWasAJoker = false;
        //test code
        BeginGame();
    }

    IEnumerator DrawStartCards()
    {
        chipMan.SetTrigger("Thumbup");
        yield return new WaitForSecondsRealtime(1.2f);
        chipMan.SetTrigger("Normal");
        dlrScoreDisplay.GetComponent<RectTransform>().localPosition = dScoreDispOrigPos;
        plrScoreDisplay.GetComponent<RectTransform>().localPosition = pScoreDispOrigPos;
        dlrScoreDisplay.text = plrScoreDisplay.text = "= 0";
        float prog = 0f;
        while (prog < 1f)
        {
            Vector3 fromPHBPos = plrHandBoxOrigPos + new Vector3(0, -100, 0);
            Vector3 fromDHBPos = dlrHandBoxOrigPos + new Vector3(0, 100, 0);
            float tween = EasingOfAccess.BounceIn(prog,0.8f);
            plrHandBox.localPosition = Vector3.Lerp(fromPHBPos, plrHandBoxOrigPos, tween);
            dlrHandBox.localPosition = Vector3.Lerp(fromDHBPos, dlrHandBoxOrigPos, tween);
            prog += 0.01666666f;
            yield return new WaitForEndOfFrame();
        }
        yield return StartCoroutine(DrawCard(1, WhoseCard.Dealer, false, false));
        yield return StartCoroutine(DrawCard(2, WhoseCard.Player, false, false));
        gameProgress = 2;
    }

    IEnumerator PlayerHit()
    {
        yield return StartCoroutine(DrawCard(1, WhoseCard.Player, false, true));
        if (gameProgress == 3)
        {
            gameProgress = 2;
        }
    }



    IEnumerator DealersTurn()
    {
        yield return new WaitForSeconds(1f);
        gameProgress = 6;
        yield return new WaitForSeconds(0.5f);
        while (dlrScore < 17 && gameProgress == 6)
        {
            yield return StartCoroutine(DrawCard(1, WhoseCard.Dealer, false, true));
            yield return new WaitForSeconds(0.2f);
        }
        if (gameProgress == 6)
        {
            gameProgress = 7;
            yield return new WaitForSeconds(1.4f);
            yield return StartCoroutine(Liftoff());
            gameProgress = 8;
            double cps = (plrScore > 21) ? Mathf.NegativeInfinity : plrScore;
            double cds = (dlrScore > 21) ? Mathf.NegativeInfinity : dlrScore;
            if (cps > cds)
            {
                endState = EndState.Win;
                Utilities.ChangeScore(Mathf.RoundToInt(250000 * Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier));
                chipMan.SetTrigger("Clap");
                StartCoroutine(EndClap());
            }
            else if (cps == cds)
            {
                endState = EndState.Tie;
                Utilities.ChangeScore(Mathf.RoundToInt(90000 * Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier));
                chipMan.SetTrigger("Normal");
            }
            else
            {
                endState = EndState.Loze;
                Utilities.ChangeScore(Mathf.RoundToInt(Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier));
                chipMan.SetTrigger("Shrug");
                StartCoroutine(EndClap());
            }
            GameObject msg = Instantiate(endStateBoxes[(int)endState - 1]);
            msg.transform.SetParent(importantUIs, true);
        }
    }

    public void BeginGame()
    {
        gameProgress = 1;
        plrCards.Clear();
        dlrCards.Clear();
        foreach (Transform t in pCardCntnr)
        {
            if (t != pcardInUISample.transform)
            {
                Destroy(t.gameObject);
            }
        }
        foreach (Transform t in dCardCntnr)
        {
            if (t != dcardInUISample.transform)
            {
                Destroy(t.gameObject);
            }
        }
        StartCoroutine(DrawStartCards());
    }

	void Update () {
        Encontrolmentation control = GetComponent<Encontrolmentation>();
        if (gameProgress == 2)
        {
            if (plrScore >= 21)
            {
                gameProgress = 4;
            }
            else
            {

                hitStandBox.sizeDelta = Vector2.Lerp(hitStandBox.sizeDelta, new Vector2(160, 16), 0.15f);
                
                if (control.ButtonDown(32UL, 96UL))
                {
                    gameProgress = 3;
                    StartCoroutine(PlayerHit());
                }
                if (control.ButtonDown(64UL, 96UL))
                {
                    gameProgress = 4;
                }
            }
        }
        else
        {
            hitStandBox.sizeDelta = Vector2.Lerp(hitStandBox.sizeDelta, new Vector2(0, 16), 0.25f);
        }
        

        if (gameProgress == 4)
        {
            gameProgress = 5;
            StartCoroutine(DealersTurn());
        }



        if (gameProgress == 6 && endState != EndState.Joker)
        {
            dealersTurnBox.sizeDelta = Vector2.Lerp(dealersTurnBox.sizeDelta, new Vector2(160, 16), 0.25f);
        }
        else
        {
            dealersTurnBox.sizeDelta = Vector2.Lerp(dealersTurnBox.sizeDelta, new Vector2(0, 16), 0.25f);
        }

        awaitChoiceSound.pitch = Mathf.Max(hitStandBox.sizeDelta.x / 160f, dealersTurnBox.sizeDelta.x / 240f);
    }

    public void OnTextBoxDeactivate()
    {
        BeginGame();
    }
}
