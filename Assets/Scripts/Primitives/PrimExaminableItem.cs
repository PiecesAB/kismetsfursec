using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrimExaminableItem : ColliderZoneTemplate<PrimExaminableItem> //playing a mean trick on the compiler for static variables in subclasses
{
    public string eventName;
    public GameObject[] textboxesToGive;
    public bool autoGiveMsg = false;
    public bool giveMessagesInOrder = false;
    public bool deleteAfterMsg = false;
    public bool destroyAfterAllMessages = false;
    public bool animateWhenClose = false;
    public int animTimer;
    public int maxAnimTimer;
    public int animFrameChangeInterval;
    public SpriteRenderer animatedSpriteOverride = null;
    public Sprite awaySprite;
    public Sprite[] animationSprites;
    public GameObject actionObj;
    public Animator animOpen;
    public WaypointPerson waypointManager;
    [Header("---------------------------------")]
    public bool itemGiver = false;
    public bool itemGiverOpened = false;
    public GameObject itemDialogPrefab;
    public Color itemDialogColor;
    //invisible
    public List<string> itemCategoriesNameMaker = new List<string>() { "none" };
    public List<sbyte> itemCategoriesWeightMaker = new List<sbyte>() { 1 };
    public Dictionary<string, sbyte> itemCategories = new Dictionary<string, sbyte>();
    [Header("<itemname>")]
    public string noItemGetMsg;
    public string itemGetMsg;
    public string alreadyOpenedMsg;
    public bool requirePlayerInput = true; // if false, NPCs may also use this

    public const int ITEMGIVER_LEVELDATA_ID = 3851920;

    [HideInInspector]
    public int textboxesGiven = 0;

    public override void ResetStuff()
    {
        return;
    }

    public override void ColliderAdd()
    {
        return;
    }

    public override void ColliderRemove(int index)
    {
        return;
    }

    public void ExtraStart()
    {
        if (animOpen)
        {
            animOpen.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        for (int i = 0; i < itemCategoriesNameMaker.Count; i++)
        {
            itemCategories.Add(itemCategoriesNameMaker[i], itemCategoriesWeightMaker[i]);
        }

        //print(itemGiver);
        if (itemGiver)
        {
            int testOpened = 0;
            Utilities.GetPersistentData(gameObject, 0, out testOpened);
            if (!Utilities.replayLevel && testOpened == ITEMGIVER_LEVELDATA_ID)
            {
                if (animOpen)
                {
                    animOpen.Play("Open");
                    animOpen.SetTrigger("Open");
                }
                itemGiverOpened = true;
            }
        }
    }

    void FixedUpdate()
    {
       if ((GetComponent<SpriteRenderer>() || animatedSpriteOverride) && animationSprites.Length > 0)
       {
           SpriteRenderer sr = animatedSpriteOverride ? animatedSpriteOverride : GetComponent<SpriteRenderer>();
           animTimer--;
           if (animTimer < 0)
           {
               animTimer = 0;
           }

           if (animTimer > 0)
           {
                sr.sprite = animationSprites[animTimer / animFrameChangeInterval];
           }
           else
           {
                sr.sprite = awaySprite;
           }
       }
    }

    public static void ItemGiverMakeOpen(PrimExaminableItem pei)
    {
        if (pei.animOpen)
        {
            if (pei.GetComponent<AudioSource>()) { pei.GetComponent<AudioSource>().Play(); }
            pei.animOpen.SetTrigger("Open");
        }
        pei.itemGiverOpened = true;
        if (pei.GetComponent<primRevealLocalID>() && !Utilities.replayLevel)
        {
            Utilities.ChangePersistentData(pei.gameObject, ITEMGIVER_LEVELDATA_ID);
        }
    }

    public static bool EvaluateConditionals(GameObject mainObject)
    {
        InGameConditional[] cons = mainObject.GetComponents<InGameConditional>();
        if (cons.Length == 0) { return true; }
        else // conjunctive: please satisfy all conditionals of this object
        {
            for (int i = 0; i < cons.Length; ++i)
            {
                if (!cons[i].Evaluate()) { return false; }
            }
            return true;
        }
    }

    private bool IsVisible()
    {
        Renderer r = GetComponent<Renderer>();
        return !r || r.isVisible;
    }

    public override void ObjectIn(int index, GameObject obj, GameObject other)
    {
        Encontrolmentation e = obj.GetComponent<Encontrolmentation>();
        PrimExaminableItem pei = other.GetComponent<PrimExaminableItem>();
        bool weaponAiming = obj.GetComponent<SpecialGunTemplate>()?.isAiming ?? false;

        if (pei && pei.animateWhenClose)
        {
            SpriteRenderer psr = animatedSpriteOverride ? animatedSpriteOverride : other.GetComponent<SpriteRenderer>();
            if (psr)
            {
                psr.sprite = pei.animationSprites[pei.animTimer / pei.animFrameChangeInterval];

                pei.animTimer += 2;
                if (pei.animTimer > pei.maxAnimTimer)
                {
                    pei.animTimer = pei.maxAnimTimer;
                }
            }
        }

        bool plrInputTest = (e?.allowUserInput ?? false) || !(pei?.requirePlayerInput ?? false);
        if (e && plrInputTest && (!weaponAiming || (pei?.autoGiveMsg ?? false)) && pei && EvaluateConditionals(pei.gameObject) && pei.IsVisible())
        {
            if (e.allowUserInput)
            {
                e.eventBbutton = Encontrolmentation.ActionButton.XButton;
                e.eventBName = pei.eventName;
            }

            if (Time.timeScale > 0 && plrInputTest && (e.ButtonDown(64UL, 64UL, !pei.requirePlayerInput) || pei.autoGiveMsg))
            {

                if (pei.itemGiver)
                {
                    bool fullInv = InventoryItemsNew.InventoryFullHandling(true);
                    //print(fullInv);
                    bool makeDialog = false;
                    //determine item
                    //if (!fullInv)
                    {
                        string myMsg = pei.itemGetMsg;
                        if (!pei.itemGiverOpened)
                        {
                            List<int> categoryRoulette = new List<int>();
                            string[] choiceGet = pei.itemCategories.Keys.ToArray();
                            for (int i = 0; i < choiceGet.Length; i++)
                            {
                                int m = pei.itemCategories[choiceGet[i]];
                                categoryRoulette.AddRange(Enumerable.Repeat(i, m));
                            }
                            string myChoiceCatName = choiceGet[categoryRoulette[Fakerand.Int(0, categoryRoulette.Count)]];
                            int myChoiceId = -1;
                            if (myChoiceCatName != "none" && !fullInv)
                            {
                                if (!int.TryParse(myChoiceCatName, out myChoiceId))
                                {
                                    int[] myChoiceIds = InventoryItemsNew.itemCategories[myChoiceCatName];
                                    myChoiceId = myChoiceIds[Fakerand.Int(0, myChoiceIds.Length)];
                                }
                                
                                //give item
                                InventoryItemsNew.AddItem(myChoiceId);
                                myMsg = myMsg.Replace("<itemname>", InventoryItemsNew.GetItemInfo(myChoiceId, true).name);
                                makeDialog = true;
                                ItemGiverMakeOpen(pei);
                            }
                            else if (myChoiceCatName == "none")//no item
                            {
                                myMsg = pei.noItemGetMsg;
                                makeDialog = true;
                                ItemGiverMakeOpen(pei);
                            }
                            else
                            {
                                InventoryItemsNew.InventoryFullHandling(false, other);
                            }
                            
                        }
                        else
                        {
                            myMsg = pei.alreadyOpenedMsg;
                            makeDialog = true;
                        }

                        if (makeDialog)
                        {
                            GameObject ne = TextBoxGiverHandler.SpawnNewBox(ref pei.itemDialogPrefab, other);
                            MainTextsStuff mts = ne.transform.GetChild(0).GetComponent<MainTextsStuff>();
                            mts.messages = new List<MainTextsStuff.MessageData>() { new MainTextsStuff.MessageData(myMsg, myMsg.Length * 0.16f, itemDialogColor) };
                        }
                    }
                }
                else
                {
                    if (pei.textboxesToGive.Length > 0)
                    {
                        if (pei.GetComponent<IDialogGiverSetup>() != null)
                        {
                            pei.GetComponent<IDialogGiverSetup>().SetupDialog();
                        }
                        GameObject ne = null;
                        if (pei.giveMessagesInOrder) //give the first one
                        {
                            ne = Instantiate(pei.textboxesToGive[0], Vector3.zero, Quaternion.identity);
                        }
                        else // random box
                        {
                            ne = Instantiate(pei.textboxesToGive[Fakerand.Int(0, pei.textboxesToGive.Length)], Vector3.zero, Quaternion.identity);
                        }
                        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
                        ne.SetActive(true);

                        if (animOpen && !itemGiver)
                        {
                            animOpen.Play("Open");
                        }
                    }
                }

                if (pei.actionObj)
                {
                    Component[] comps = pei.actionObj.GetComponents(typeof(IExaminableAction));
                    for (int c = 0; c < comps.Length; ++c)
                    {
                        ((IExaminableAction)comps[c]).OnExamine(e);
                    }
                }

                if (pei.giveMessagesInOrder && pei.textboxesToGive.Length > 0)
                {
                    // remove the first element
                    GameObject[] newTextsToGive = new GameObject[pei.textboxesToGive.Length - 1];
                    for (int i = 1; i < pei.textboxesToGive.Length; ++i)
                    {
                        newTextsToGive[i - 1] = pei.textboxesToGive[i];
                    }
                    pei.textboxesToGive = newTextsToGive;
                    ++pei.textboxesGiven;
                }

                if (pei.deleteAfterMsg || (pei.giveMessagesInOrder && pei.textboxesToGive.Length == 0))
                {
                    Destroy(pei);
                }

                if (pei.destroyAfterAllMessages && pei.textboxesToGive.Length == 0)
                {
                    GenericBlowMeUp bmu = pei.GetComponent<GenericBlowMeUp>();
                    if (bmu) { bmu.BlowMeUp(0.05f); } else { Destroy(pei.gameObject); }
                }

                if (pei.waypointManager)
                {
                    pei.waypointManager.MoveToNextWaypoint();
                }
            }
        }
    }
}
