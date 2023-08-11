using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryControlNew : MonoBehaviour
{
    public GameObject entireScrollBox;
    public RectTransform cursor;
    public RectTransform inventoryArea;
    public GameObject itemPanelSample;
    public Color noselectPanelColor;
    public Color selectPanelColor;
    public Color activePanelColor;
    public Color lockedPanelColor;
    public Sprite noItemImage;
    public RectTransform nameBox;
    public Text nameBoxText;
    public RectTransform descBox;
    public MainTextsStuff descBoxText;
    public RectTransform instrucBox;
    public RectTransform activeSlotPic;
    public Text[] instrucTexts;
    public LiteratureMenu literatureMenu;

    public AudioSource openSound;
    public AudioSource changeSound;
    public AudioSource backSound;
    public AudioSource selectSound;
    public AudioSource activateSound;
    public AudioSource throwawaySound;

    private string[] instrucTextsDefault;

    private GameObject[] itemPanels = new GameObject[25];
    private string[] itemNames = new string[25];
    private string[] itemDescs = new string[25];
    private Vector2[] panelTargPositions = new Vector2[25];
    private int activePanel = -1;

    private const string lockedNoitemMsg = "X Score is needed to use this slot.";

    private Encontrolmentation e;
    private Image im;
    private const float cursorSpeed = 2f;
    private float cursorBoost = 0f;
    private float cursorBoostMax = 4f;
    private const float cursorBoostMultiplier = 1.5f;
    private float scrollToCursorMultiplier;
    private const float scrollToCursorMultiplierS = 0.3f;
    private const float scrollToCursorMultiplierA = 0.5f;
    private static Vector2 cursorDefaultPosition = new Vector2(0, 0);

    private const float openBlocksDistAway = 480f;
    private const float noselectSize = 48f;
    private const float selectSize = 64f;
    private const float activeSize = 96f;
    private const float nameboxYOffset = 48f;
    private const float descboxWidth = 256f;
    private static float descboxYOffset = 88f;
    private const float instrucboxHeight = 80f;
    private const float instrucboxXOffset = 104f;
    private const float instrucboxYOffset = 40f;

    private static Color instrucDisabledColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private float oldTimeScaleStore = 1f;


    private static readonly int[] spiralX = new int[25]
    {
         0, 1, 1, 0,-1,-1,-1, 0, 1, 2, 2, 2, 2, 1, 0,-1,-2,-2,-2,-2,-2,-1, 0, 1, 2
    };
    private static readonly int[] spiralY = new int[25]
    {
         0, 0, 1, 1, 1, 0,-1,-1,-1,-1, 0, 1, 2, 2, 2, 2, 2, 1, 0,-1,-2,-2,-2,-2,-2
    };

    private bool noDelete = false;
    private bool literature = false;

    void Awake()
    {
        noDelete = false;
        entireScrollBox.SetActive(true);
        cursorBoost = 0f;
        e = GetComponent<Encontrolmentation>();
        im = GetComponent<Image>();
        //make item squares

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameObject newPanel = Instantiate(itemPanelSample, inventoryArea);
                RectTransform newRT = newPanel.GetComponent<RectTransform>();
                Image cim = newRT.GetChild(0).GetComponent<Image>();
                cim.material = new Material(cim.material);
                int k = 5 * i + j;
                panelTargPositions[k] = new Vector2(spiralX[k]*56, spiralY[k]* 56);
                itemPanels[k] = newPanel;
            }
        }
        cursor.SetAsLastSibling();
        cursor.localPosition = cursorDefaultPosition;
        nameBox.localPosition = descBox.localPosition = instrucBox.localPosition = new Vector3(10000f, 10000f, 0f);
        Destroy(itemPanelSample);

        instrucTextsDefault = new string[instrucTexts.Length];
        for (int i = 0; i < instrucTexts.Length; ++i)
        {
            instrucTextsDefault[i] = instrucTexts[i].text;
        }

        Close();
    }

    private void UpdatePanelInfo()
    {
        for (int i = 0; i < 25; i++)
        {
            Image iim = itemPanels[i].GetComponent<RectTransform>().GetChild(0).GetComponent<Image>();
            if (i < Utilities.loadedSaveData.SharedPlayerItems.Count)
            {
                InventoryItemsNew.ItemInfoHolder ifh = InventoryItemsNew.GetItemInfo(i);
                iim.sprite = ifh.imag;
                itemNames[i] = ifh.name;
                itemDescs[i] = ifh.desc;
            }
            else
            {
                iim.sprite = noItemImage;
                itemNames[i] = "None";
                itemDescs[i] = "Nothing is here.";
            }
        }
    }

    private void Open()
    {
        if (KHealth.someoneDied) { return; }
        if (Time.timeScale == 0f) { return; }
        if (!Utilities.canUseInventory) { return; }
        if (PauseMenuMain.gameIsPausedThroughMenu) { return; }

        if (PauseMenuMain.main == null || PauseMenuMain.main.PausingTooQuickCheck()) { return; };

        PauseMenuMain.gameIsPausedThroughMenu = true;

        oldTimeScaleStore = 1f;
        Time.timeScale = 0f;

        

        entireScrollBox.SetActive(true);
        cursor.localPosition = cursorDefaultPosition;
        im.enabled = true;
        im.color = new Color(1f, 1f, 1f, 0f);
        activePanel = -1;
        for (int i = 0; i < 25; i++)
        {
            RectTransform irt = itemPanels[i].GetComponent<RectTransform>();
            irt.localPosition = Fakerand.UnitCircle(true) * openBlocksDistAway;
            irt.sizeDelta = new Vector2(noselectSize, noselectSize);
        }
        scrollToCursorMultiplier = scrollToCursorMultiplierS;
        UpdatePanelInfo();
    }

    private IEnumerator CloseTime()
    {
        while (im.color.a > 0f)
        {
            im.color = new Color(1f, 1f, 1f, Mathf.Clamp01(im.color.a - 0.04f));
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = oldTimeScaleStore;
        im.enabled = false;
        yield return new WaitForEndOfFrame();

        PauseMenuMain.gameIsPausedThroughMenu = false;
    }

    private void Close(bool instant = true)
    {
        nameBox.localPosition = descBox.localPosition = instrucBox.localPosition = new Vector3(10000f, 10000f, 0f);
        entireScrollBox.SetActive(false);
        if (instant)
        {
            Time.timeScale = oldTimeScaleStore;
            im.enabled = false;
        }
        else
        {
            StartCoroutine(CloseTime());
        }
    }

    private int lastPanel = -1;

    void Update()
    {
        if (entireScrollBox.activeInHierarchy)
        {

            activeSlotPic.SetAsFirstSibling();
            if (!e)
            {
                goto endUpdate;
            }

            if (e && (e.AnyButtonDown(3072UL) || (e.ButtonDown(32UL, 240UL) && activePanel == -1)))
            {
                backSound.Stop(); backSound.Play();
                Close(false);
                goto endUpdateB;
            }

            int availablePanels = Utilities.getInventorySlotCount();

            if (activePanel == -1)
            {

                #region cursorMove
                int movDirCode = (int)(e.currentState & 15UL);
                if (movDirCode != 0 && movDirCode != 15)
                {
                    Vector3 cursorDir = Vector2.zero;
                    if ((movDirCode & 1) == 1)
                    {
                        cursorDir.x -= 1f;
                    }
                    if ((movDirCode & 2) == 2)
                    {
                        cursorDir.x += 1f;
                    }
                    if ((movDirCode & 4) == 4)
                    {
                        cursorDir.y += 1f;
                    }
                    if ((movDirCode & 8) == 8)
                    {
                        cursorDir.y -= 1f;
                    }
                    cursor.localPosition += cursorDir * (cursorSpeed + cursorBoost);
                    Vector2 invAreaExtent = new Vector2(inventoryArea.rect.width * 0.5f, inventoryArea.rect.height * 0.5f);
                    cursor.localPosition = new Vector3(Mathf.Clamp(cursor.localPosition.x, -invAreaExtent.x, invAreaExtent.x),
                                                       Mathf.Clamp(cursor.localPosition.y, -invAreaExtent.y, invAreaExtent.y),
                                                       cursor.localPosition.z);
                    cursorBoost = Mathf.Min(cursorBoostMax, cursorBoost + 0.0166666f * cursorBoostMultiplier);
                }
                else
                {
                    cursorBoost = 0f;
                }
                #endregion

                activeSlotPic.gameObject.SetActive(true);
                scrollToCursorMultiplier = Mathf.MoveTowards(scrollToCursorMultiplier, scrollToCursorMultiplierS, 0.01f);
                if (im.color.a < 1f)
                {
                    im.color = new Color(1f, 1f, 1f, Mathf.Clamp01(im.color.a + 0.04f));
                }
                for (int i = 0; i < 25; i++)
                {
                    RectTransform irt = itemPanels[i].GetComponent<RectTransform>();
                    if (((Vector2)irt.localPosition-panelTargPositions[i]).SqrMagnitude() > 1f)
                    {
                        irt.localPosition = Vector3.Lerp(irt.localPosition, panelTargPositions[i], 0.15f);
                    }
                    else
                    {
                        irt.localPosition = panelTargPositions[i];
                    }
                    
                    Image iim = itemPanels[i].GetComponent<Image>();
                    Vector3Int vimin = new Vector3Int(Mathf.RoundToInt(irt.localPosition.x + irt.rect.xMin), Mathf.RoundToInt(irt.localPosition.y + irt.rect.yMin), -1);
                    Vector3Int visiz = new Vector3Int(Mathf.RoundToInt(irt.rect.size.x), Mathf.RoundToInt(irt.rect.size.y), 2);
                    BoundsInt ibi = new BoundsInt(vimin, visiz);
                    float ms = irt.sizeDelta.x;
                    if (ibi.Contains(new Vector3Int(Mathf.RoundToInt(cursor.localPosition.x), Mathf.RoundToInt(cursor.localPosition.y), 0)))
                    {
                        if (lastPanel != i) { changeSound.Stop(); changeSound.Play(); }
                        lastPanel = i;
                        if (e.ButtonDown(16UL, 240UL))
                        {
                            selectSound.Stop(); selectSound.Play();
                            for (int j = 0; j < 25; j++)
                            {
                                if (j != i)
                                {
                                    Image jim = itemPanels[j].GetComponent<Image>();
                                    Image cjm = jim.transform.GetChild(0).GetComponent<Image>();
                                    jim.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                                    cjm.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                                }
                            }
                            nameBoxText.text = itemNames[i];
                            nameBox.localPosition = irt.localPosition + Vector3.up * ((spiralY[i] < 0) ? nameboxYOffset : -nameboxYOffset);
                            descBox.localPosition = new Vector3(irt.localPosition.x * 0.5f, irt.localPosition.y + ((spiralY[i] < 0) ? descboxYOffset : -descboxYOffset));
                            instrucBox.localPosition = new Vector3(irt.localPosition.x + instrucboxXOffset * ((spiralX[i] < 1) ? 1 : -1),
                                                                   irt.localPosition.y * 0.66f + instrucboxYOffset * ((spiralY[i] > -1) ? 1 : -1));
                            nameBox.sizeDelta = new Vector2(0f, nameBox.sizeDelta.y);
                            descBox.sizeDelta = new Vector2(0f, descBox.sizeDelta.y);
                            instrucBox.sizeDelta = new Vector2(instrucBox.sizeDelta.x, 0f);

                            Color boxColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                            if (i >= availablePanels)
                            {
                                boxColor = new Color(0.25f, 0f, 0f, 1f);
                            }
                            else if (i == 0)
                            {
                                boxColor = new Color(0.25f, 0f, 0.25f, 1f);
                            }

                            nameBox.GetComponent<Image>().color = descBox.GetComponent<Image>().color = instrucBox.GetComponent<Image>().color = boxColor;
                            activePanel = i;
                            activeSlotPic.gameObject.SetActive((activePanel == 0) ? true : false);
                            if (i < availablePanels)
                            {
                                descBoxText.messages[0] = new MainTextsStuff.MessageData(itemDescs[i], 999999999999f, descBoxText.messages[0].defaultColor, 0f);
                                iim.color = activePanelColor;
                            }
                            else
                            {
                                if (itemDescs[i] == "Nothing is here.")
                                {
                                    string Xnum = (Utilities.scoreForInventorySlot[i] >= 1000000) ?
                                        (string.Format("{0:0.##}", (Utilities.scoreForInventorySlot[i] / 1000000f)) + " million") :
                                        ((Utilities.scoreForInventorySlot[i] / 1000).ToString() + " thousand");
                                    descBoxText.messages[0] = new MainTextsStuff.MessageData(lockedNoitemMsg.Replace("X", Xnum),
                                                                                        999999999999f, descBoxText.messages[0].defaultColor, 0f);
                                }
                                else
                                {
                                    descBoxText.messages[0] = new MainTextsStuff.MessageData(itemDescs[i], 999999999999f, descBoxText.messages[0].defaultColor, 0f);
                                }
                                iim.color = lockedPanelColor;
                            }

                            for (int k = 0; k < instrucTexts.Length; ++k)
                            {
                                instrucTexts[k].text = instrucTextsDefault[k];
                            }
                            literature = false;

                            if (activePanel >= 0 && activePanel < Utilities.loadedSaveData.SharedPlayerItems.Count)
                            {
                                Dictionary<string, object> objData = InventoryItemsNew.allItems[Utilities.loadedSaveData.SharedPlayerItems[activePanel]];
                                noDelete = objData.ContainsKey("nondeletable");
                                //  instrucTexts[0] should be X button message
                                literature = objData.ContainsKey("literature");
                                if (literature)
                                {
                                    instrucTexts[0].text = "Observe";
                                }
                            }

                            descBoxText.Begin();
                            break;
                        }
                        else
                        {
                            if (i < availablePanels)
                            {
                                iim.color = selectPanelColor;
                            }
                            else
                            {
                                iim.color = lockedPanelColor;
                            }
                            ms = Mathf.MoveTowards(ms, selectSize, 2f);
                            Image cim = irt.GetChild(0).GetComponent<Image>();
                            cim.material.SetVector("_TeSi", new Vector4(ms, ms));
                            cim.color = Color.white;
                        }
                    }
                    else
                    {
                        if (i < availablePanels)
                        {
                            iim.color = noselectPanelColor;
                        }
                        else
                        {
                            iim.color = lockedPanelColor;
                        }
                        ms = Mathf.MoveTowards(ms, noselectSize, 2f);
                        Image cim = irt.GetChild(0).GetComponent<Image>();
                        cim.material.SetVector("_TeSi", new Vector4(ms, ms));
                        cim.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                    irt.sizeDelta = new Vector2(ms, ms);
                }
            }
            else
            {

                instrucTexts[0].color = instrucTexts[1].color = instrucDisabledColor;
                if (activePanel >= 0 && activePanel < Utilities.loadedSaveData.SharedPlayerItems.Count)
                {
                    if (!noDelete)
                    {
                        instrucTexts[1].color = Color.white;
                    }

                    if (activePanel >= 1 || literature)
                    {
                        instrucTexts[0].color = Color.white;
                    }
                }

                nameBox.sizeDelta = new Vector2(Mathf.MoveTowards(nameBox.sizeDelta.x, nameBoxText.preferredWidth + 32f, 6f), nameBox.sizeDelta.y);
                descBox.sizeDelta = new Vector2(Mathf.MoveTowards(descBox.sizeDelta.x, descboxWidth, 12f), descBox.sizeDelta.y);
                instrucBox.sizeDelta = new Vector2(instrucBox.sizeDelta.x, Mathf.MoveTowards(instrucBox.sizeDelta.y, instrucboxHeight, 6f));
                if (e.ButtonDown(32UL, 240UL)) //back
                {
                    backSound.Stop(); backSound.Play();
                    activePanel = -1;
                }
                else if (e.ButtonDown(16UL, 240UL)) //swap
                {
                    selectSound.Stop(); selectSound.Play();
                    if (literature)
                    {
                        Dictionary<string, object> objData = InventoryItemsNew.allItems[Utilities.loadedSaveData.SharedPlayerItems[activePanel]];
                        literatureMenu.textLink = (string)objData["literature"];
                        literatureMenu.imageLink = (string)objData["imag"];
                        literatureMenu.titleText = (string)objData["name"];
                        literatureMenu.Open(e);
                    }
                    else if (activePanel >= 1 && activePanel < Utilities.loadedSaveData.SharedPlayerItems.Count) //swap items
                    {
                        int transfer = Utilities.loadedSaveData.SharedPlayerItems[activePanel];
                        InventoryItemsNew.DeleteItemByIndex(activePanel);
                        InventoryItemsNew.AddItem(transfer, true);
                        UpdatePanelInfo();
                        activePanel = -1;
                        //cursor.localPosition = cursorDefaultPosition;
                    }
                }
                else if ((e.currentState & 240UL) == 64UL) //delete
                {
                    if (activePanel >= 0 && activePanel < Utilities.loadedSaveData.SharedPlayerItems.Count)
                    {
                        if (!noDelete)
                        {
                            RectTransform art = itemPanels[activePanel].GetComponent<RectTransform>();
                            float ms = art.sizeDelta.x;
                            ms = Mathf.MoveTowards(ms, 0f, 2f);
                            Image cim = art.GetChild(0).GetComponent<Image>();
                            cim.material.SetVector("_TeSi", new Vector4(ms, ms));
                            art.sizeDelta = new Vector2(ms, ms);
                            if (ms == 0f)
                            {
                                throwawaySound.Stop(); throwawaySound.Play();
                                InventoryItemsNew.DeleteItemByIndex(activePanel);
                                art.sizeDelta = new Vector2(noselectSize, noselectSize);
                                UpdatePanelInfo();
                                activePanel = -1;
                            }
                        }
                    }
                }
                else
                {
                    RectTransform art = itemPanels[activePanel].GetComponent<RectTransform>();
                    art.SetAsLastSibling();
                    float ms = art.sizeDelta.x;
                    ms = Mathf.MoveTowards(ms, activeSize, 4f);
                    Image cim = art.GetChild(0).GetComponent<Image>();
                    cim.material.SetVector("_TeSi", new Vector4(ms, ms));
                    cim.color = Color.white;
                    art.sizeDelta = new Vector2(ms, ms);
                    cursor.localPosition = Vector3.MoveTowards(cursor.localPosition, art.localPosition, 4f);
                    scrollToCursorMultiplier = Mathf.MoveTowards(scrollToCursorMultiplier, scrollToCursorMultiplierA, 0.01f);
                }
            }

            endUpdate:

            inventoryArea.localPosition = -cursor.localPosition * scrollToCursorMultiplier;
            if (activePanel == 0)
            {
                activeSlotPic.SetAsLastSibling();
            }
            cursor.SetAsLastSibling();
            nameBox.SetAsLastSibling();
            descBox.SetAsLastSibling();
            instrucBox.SetAsLastSibling();
            if (activePanel == -1)
            {
                nameBox.localPosition = descBox.localPosition = instrucBox.localPosition = new Vector3(10000f, 10000f, 0f);
                cursor.GetComponent<Image>().color = Color.white;
            }
            else
            {
                cursor.GetComponent<Image>().color = Color.clear;
            }
            activeSlotPic.sizeDelta = itemPanels[0].GetComponent<RectTransform>().sizeDelta + new Vector2(16, 16);

        endUpdateB:
            ;
        }
        else
        {
            if (e && e.ButtonDown(2048UL, 3072UL))
            {
                openSound.Stop(); openSound.Play();
                Open();
            }
        }
    }
}
