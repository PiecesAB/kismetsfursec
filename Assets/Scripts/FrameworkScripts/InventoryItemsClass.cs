using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class InventoryItemsNew
{
    public struct FakerandEvaluator<T>
    {
        public T min;
        public T max;
        public Func<T, T, T> func;

        public FakerandEvaluator(T _min, T _max, Func<T, T, T> _func)
        {
            min = _min;
            max = _max;
            func = _func;
        }

        public T Evaluate()
        {
            return func(min, max);
        }

        public static implicit operator T(FakerandEvaluator<T> x)
        {
            return x.Evaluate();
        }
    }

    public struct CustomFunc //just a wrapper
    {
        public Func<string> func;

        public CustomFunc(Func<string> inFunc)
        {
            func = inFunc;
        }

        public string Evaluate()
        {
            return func();
        }
    }

    public static Sprite fallbackImage = Resources.Load<Sprite>("InventoryPics/fallback");
    public static Sprite corruptedImage = Resources.Load<Sprite>("InventoryPics/corrupted");
    public static GameObject dialogPrefab = Resources.Load<GameObject>("InventoryPics/dialogPrefab");
    public static GameObject slideRoulettePrefab = Resources.Load<GameObject>("InventoryPics/slideRoulettePrefab");
    public static string dialogMessage = "";
    public static GameObject eatEffect = null;

    private const string fullInventoryMessage = "Full Inventory! To collect the item here, increase Score, or be rid of other items.";

    public struct ItemInfoHolder
    {
        public string name;
        public string desc;
        public Sprite imag;

        public ItemInfoHolder(object _name, object _description, object _imagePath)
        {
            if (_name is string)
            {
                name = _name as string;
            }
            else
            {
                name = "";
                for (int a = 0; a < Fakerand.Int(4, 24); a++)
                {
                    name += (char)Fakerand.Int(32, 127);
                }
            }

            if (_description is string)
            {
                desc = _description as string;
            }
            else
            {
                string[] d1 = new string[] { "Where are your ", "Do you have ", "Did you forget your " };
                string[] d2 = new string[] { "teeth", "children", "manners", "guns", "bodies", "lives", "new eyes" };
                desc = d1[Fakerand.Int(0, d1.Length)] + d2[Fakerand.Int(0, d2.Length)] + "?";
            }

            if (_imagePath is string)
            {
                Sprite tempImag = Resources.Load<Sprite>(_imagePath as string);
                if (tempImag)
                {
                    imag = tempImag;
                }
                else
                {
                    imag = fallbackImage;
                }
            }
            else
            {
                imag = corruptedImage;
            }
        }
    }

    private static void MakeDialog(GameObject mainSpeaker)
    {
        GameObject ne = UnityEngine.Object.Instantiate(dialogPrefab, Vector3.zero, Quaternion.identity);
        if (mainSpeaker) { ne.GetComponentInChildren<TextBoxGiverHandler>().mainSpeaker = mainSpeaker; }
        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
        ne.transform.GetChild(0).GetComponent<MainTextsStuff>().messages
                    = new List<MainTextsStuff.MessageData>() { new MainTextsStuff.MessageData(dialogMessage, dialogMessage.Length * 0.16f) };
        ne.SetActive(true);
    }

    public static bool InventoryHasItem(int id)
    {
        int garbage = -1;
        return InventoryHasItem(id, out garbage);
    }

    public static bool InventoryHasItem(int id, out int index)
    {
        for (int i = 0; i < Utilities.loadedSaveData.SharedPlayerItems.Count; ++i)
        {
            if (Utilities.loadedSaveData.SharedPlayerItems[i] == id) { index = i; return true; }
        }
        index = -1;
        return false;
    }

    private static bool customFuncDeleteSkip = false;

    public static readonly Dictionary<int, Dictionary<string, object>> allItems = new Dictionary<int, Dictionary<string, object>>()
    {
        { 0,
          new Dictionary<string, object>()
            {
                {"name", "Empty Vessel" },
                {"desc", "Please don't delete me. They just took my body. <wave>I promise I'll get it back."},
                {"imag", "InventoryPics/emptyVessel"},

                {"nonconsumable", null},
            }
        },

        { 1,
          new Dictionary<string, object>()
            {
                {"name", "Yazop Coin" },
                {"desc", "Medallion only owned by members of the Yazop family. Worth one-decillionth of a picoscore."},
                {"imag", "InventoryPics/yazopCoin"},

                {"nonconsumable", null},
            }
        },

        { 2,
          new Dictionary<string, object>()
            {
                {"name", "Addend" },
                {"desc", "Duplicate the item that is to the right of the active slot."},
                {"imag", "InventoryPics/addend"},

                {"custom func", new CustomFunc(() => {
                    customFuncDeleteSkip = true;
                    if (Utilities.loadedSaveData.SharedPlayerItems.Count < 2)
                    {
                        return "There is no other item to clone.";
                    }
                    int copyItem = Utilities.loadedSaveData.SharedPlayerItems[1];
                    if (allItems.ContainsKey(copyItem) && allItems[copyItem].ContainsKey("nonconsumable"))
                    {
                        return "Can't clone special non-consumables.";
                    }
                    Utilities.loadedSaveData.SharedPlayerItems[0] = copyItem;
                    string copyItemName = allItems.ContainsKey(copyItem)?(string)allItems[copyItem]["name"]:"???";
                    return "Cloned <!>" + copyItemName + "<!> to active slot.";
                }) },
            }
        },

        { 3,
          new Dictionary<string, object>()
            {
                {"name", "Multiplicand" },
                {"desc", "Replace all its items with the one right of the active slot."},
                {"imag", "InventoryPics/multiplicand"},

                {"custom func", new CustomFunc(() => {
                    customFuncDeleteSkip = true;
                    if (Utilities.loadedSaveData.SharedPlayerItems.Count < 2)
                    {
                        return "There is no other item to clone.";
                    }
                    int copyItem = Utilities.loadedSaveData.SharedPlayerItems[1];
                    if (allItems.ContainsKey(copyItem) && allItems[copyItem].ContainsKey("nonconsumable"))
                    {
                        return "Can't clone special non-consumables.";
                    }
                    for (int i = 0; i < Utilities.loadedSaveData.SharedPlayerItems.Count; ++i)
                    {
                        Utilities.loadedSaveData.SharedPlayerItems[i] = copyItem;
                    }
                    string copyItemName = allItems.ContainsKey(copyItem)?(string)allItems[copyItem]["name"]:"???";
                    return "Replaced all its items with <!>" + copyItemName + "<!> .";
                }) },
            }
        },

        { 100,
          new Dictionary<string, object>()
            {
                {"name", "Three-Glass" },
                {"desc", "Looks like a hard sell. Maybe it can see invisible blocks and zones."},
                {"imag", "InventoryPics/threeGlass"},
                {"used msg", "Equipped three-glass! It fits incredibly poorly."},

                {"reveal", null},
            }
        },

        { 101,
          new Dictionary<string, object>()
            {
                {"name", "Brass Paperweight" },
                {"desc", "An oddly shaped paperweight. Somehow increases melee power for the level."},
                {"imag", "InventoryPics/brassPaperweight"},
                {"used msg", "Equipped brass paperweight. Good thing one doesn't need thumbs to hold it like that."},

                {"melee power", 4f},
            }
        },

        { 102,
          new Dictionary<string, object>()
            {
                {"name", "Small Pantograph" },
                {"desc", "Did it ever want to be small? Get crushed by friends?? That's so weird..."},
                {"imag", "InventoryPics/pantographSmall"},

                {"size mul", 0.5f},
            }
        },

        { 103,
          new Dictionary<string, object>()
            {
                {"name", "Big Pantograph" },
                {"desc", "Did it ever want to be big? And crush its friends?? That's so weird..."},
                {"imag", "InventoryPics/pantographBig"},

                {"size mul", 2f},
            }
        },

        { 104,
          new Dictionary<string, object>()
            {
                {"name", "Slide Roulette" },
                {"desc", "Portable gambling device. It's free score!"},
                {"imag", "InventoryPics/slideRoulette"},

                {"slide roulette", null},
            }
        },

        { 105,
          new Dictionary<string, object>()
            {
                {"name", "Qi Squared" },
                {"desc", "Allows a Being to refresh without resurrecting. Take up to 30 damage more than lethal to activate."},
                {"imag", "InventoryPics/qiSquared"},

                { "unusable", "This can't be used. It will activate automatically on the destruction of Being." },
                { "custom func", new CustomFunc(() => {
                    Encontrolmentation e = LevelInfoContainer.GetActiveControl();
                    if (e == null) { return ""; }
                    GameObject effect = GameObject.Instantiate(Resources.Load<GameObject>("QiSquaredEffect"),
                        e.transform.position, Quaternion.identity);
                    return "";
                }) },
            }
        },

        { 200,
          new Dictionary<string, object>()
            {
                {"name", "Calorie Cube" },
                {"desc", "Single calorie default food."},
                {"imag", "InventoryPics/calorieCube2"},
                {"used msg", "Tastes vaguely like soymilk! Damage -20"},

                {"heal", 20f},
            }
        },

        { 201,
          new Dictionary<string, object>()
            {
                {"name", "Blue Apple" },
                {"desc", "The apple of tomorrow! Delete 20 Damage, and jump slightly higher."},
                {"imag", "InventoryPics/blueApple"},
                {"used msg", "Now avoid those blue doctors! Damage -20. Jump +15."},

                {"heal", 20f},
                {"jump power", 15f},
            }
        },

        { 202,
          new Dictionary<string, object>()
            {
                {"name", "Gisnep Cola" },
                {"desc", "Even after being bought by Gisnep, they still make it with real drugs."},
                {"imag", "InventoryPics/gisnepCola"},
                {"used msg", "Tastes like copyright extension! Tired +0.99"},

                {"tired high", -0.999f},
            }
        },

        { 203,
          new Dictionary<string, object>()
            {
                {"name", "Bacon Cola" },
                {"desc", "The only cola with 100% real bacon."},
                {"imag", "InventoryPics/baconCola"},
                {"used msg", "Tastes like the rich know best! Damage -33"},

                {"heal", 33f},
            }
        },

        { 204,
          new Dictionary<string, object>()
            {
                {"name", "Pepper Pop" },
                {"desc", "The spiciest ice pop around!"},
                {"imag", "InventoryPics/pepperPop"},
                {"used msg", "Damage -60, but it's too spicy!"},

                {"heal", 60f},
                {"burn", new FakerandEvaluator<float>(0f,0.75f,Fakerand.Single)},

            }
        },

        { 405, // four hundred because the player should not be getting energy in an unintended level. this can lead to huge out of bounds problems.
          new Dictionary<string, object>()
            {
                {"name", "Dairy Dessert Cone" },
                {"desc", "Now in new flavors! This one's \"electric blue\"."},
                {"imag", "InventoryPics/dairyDessertCone"},
                {"used msg", "Tastes like how copper smells. Energy +100"},

                {"energy", 100f},

            }
        },

        { 206,
          new Dictionary<string, object>()
            {
                {"name", "Assorted Fried Objects" },
                {"desc", "This food is like life. You never know what you're going to get"},
                {"imag", "InventoryPics/assortedFriedObjects"},
                {"used msg", "Damage -55! But something feels wrong. Speed -10"},

                {"heal", 55f},
                {"move speed", -10f},
            }
        },

        { 207,
          new Dictionary<string, object>()
            {
                {"name", "Bottom Ramen" },
                {"desc", "Everyone at the Triakulus library calls it \"student food\"."},
                {"imag", "InventoryPics/bottomRamen"},
                {"used msg", "Some amount of Damage removed. Hey it's my turn to play now. When can I play the game? It looks so fun."},

                {"heal", new FakerandEvaluator<float>(10f,30f,Fakerand.Single)},
                {"tired high", -0.6f},
            }
        },

        { 208,
          new Dictionary<string, object>()
            {
                {"name", "Milkwater" },
                {"desc", "The outrageous taste of skim milk, blended with the outrageous taste of water!"},
                {"imag", "InventoryPics/milkwater"},
                {"used msg", "-14 Damage (due to the milk), Melee power plus (due to the water)!"},

                {"heal", 14f},
                {"melee power", 1f },
            }
        },

        { 209,
          new Dictionary<string, object>()
            {
                {"name", "Dip Stick" },
                {"desc", "Might help with eating candy... and smells like Calorie Cube?"},
                {"imag", "InventoryPics/dipStick"},
                {"used msg", "Damage -10. Tastes like Calorie Cube too!"},

                {"heal", 10f},
            }
        },

        { 210,
          new Dictionary<string, object>()
            {
                {"name", "Gud Dip" },
                {"desc", "This doesn't come with the stick. It would probably be easier to eat if it did."},
                {"imag", "InventoryPics/gudDip"},
                {"used msg", "+0.99 High... if the stick is obtained. Otherwise just +0.5 because it gets everywhere"},

                {"tired high", new FakerandEvaluator<float>(0.5f,0.99f,
                    (min,max) => {return InventoryHasItem(209)?max:min;}) },
            }
        },

        { 211,
          new Dictionary<string, object>()
            {
                {"name", "Whole Chicken" },
                {"desc", "A normal rotisserie chicken. No it's not radioactive or drugged or anything. Really."},
                {"imag", "InventoryPics/wholeChicken"},
                {"used msg", "It ate the whole thing in one bite. Damage -120!"},

                {"heal", 120f},
            }
        },

        { 212,
          new Dictionary<string, object>()
            {
                {"name", "Molten Lead Brick" },
                {"desc", "Anything that eats it dies."},
                {"imag", "InventoryPics/moltenLeadBrick"},
                {"used msg", "Damage +123456790. Lol crumbed."},

                {"heal", -123456790f},
            }
        },

        { 213,
          new Dictionary<string, object>()
            {
                {"name", "Geometry Mix" },
                {"desc", "The educational snack! Cherry tomato spheres, cheese cubes, ham tetrahedrons, and rat poison cylinders!"},
                {"imag", "InventoryPics/geometryMix"},
                {"used msg", "It ate everything in the mix, including the rat poison. Luckily it balances out. Net Damage reduction: -16."},

                {"heal", 16f},
            }
        },

        { 214,
          new Dictionary<string, object>()
            {
                {"name", "Mystery Syringe" },
                {"desc", "Probably drugs. But don't worry. Winners always do drugs."},
                {"imag", "InventoryPics/mysterySyringe"},
                {"used msg", "Injected the mystery syringe! Who knows what happened!"},

                {"heal", new FakerandEvaluator<float>(0f,0f,
                    (min,max) => {
                        if (Fakerand.Single() < 0.25f) { return -Mathf.Exp(Fakerand.Single()*9f); }
                        return 0f;
                    }) },
                {"move speed", new FakerandEvaluator<float>(-40f,40f,Fakerand.Single) },
                {"jump power", new FakerandEvaluator<float>(-20f,20f,Fakerand.Single) },
                {"melee power", new FakerandEvaluator<float>(0f,1f,Fakerand.Single) },
            }
        },

        { 215,
          new Dictionary<string, object>()
            {
                {"name", "Blood Sandwich" },
                {"desc", "A great way to finish the leftover type AB blood. It's still soggy and dripping."},
                {"imag", "InventoryPics/bloodSandwich"},
                {"used msg", "Damage -38. Tastes like there should be meat somewhere. But it's all blood!"},

                {"heal", 38f},
            }
        },

        { 216,
          new Dictionary<string, object>()
            {
                {"name", "Music Mud" },
                {"desc", "A bitchin' Eighthday special! Similar to mashed potatoes."},
                {"imag", "InventoryPics/musicMud"},
                {"used msg", "Embrace the power of plagiarized background music! Damage -10. Melee power plus!"},

                {"heal", 10f },
                {"melee power", 1f },
            }
        },

        { 217,
          new Dictionary<string, object>()
            {
                {"name", "Ade" },
                {"desc", "Did it ever have lemon? I don't think so. Sugar is enough."},
                {"imag", "InventoryPics/ade"},
                {"used msg", "Tastes vaguely like every edible product in the supermarket! Damage -25."},

                {"heal", 25f },
            }
        },

        { 218,
          new Dictionary<string, object>()
            {
                {"name", "Pretentious Bar" },
                {"desc", "If you don't like it, you just don't understand. Plebeian."},
                {"imag", "InventoryPics/pretentiousBar"},
                {"used msg", "Oh resplendent day! Oh youthful rejuvenation! Can one possibly feel better? Damage -2."},

                {"heal", 2f },
            }
        },



        { 600,
          new Dictionary<string, object>()
            {
                {"name", "Pizza Round (Sausage)" },
                {"desc", "Heated to perfection, made only with 100% oil"},
                {"imag", "InventoryPics/pizzaRoundSausage"},
                {"used msg", "The dough is charred outside and raw inside, but the intensely plastic \"sausage\" masks this. Damage -8"},

                {"heal", 8f},
            }
        },

        { 601,
          new Dictionary<string, object>()
            {
                {"name", "Pizza Round (Supreme)" },
                {"desc", "Heated with 100% oil; new toppings"},
                {"imag", "InventoryPics/pizzaRoundSupreme"},
                {"used msg", "*cough cough* Damage -5."},

                {"heal", 5f},
            }
        },




        { 300,
          new Dictionary<string, object>()
            {
                {"name", "Ace of Keys" },
                {"desc", "It's a key card... get it?????????????"},
                {"imag", "InventoryPics/aceOfKeys"},

                { "unusable", "Find a key pad and press ○ to unlock it. Most items are consumed using △ though." },
                {"nonconsumable", null },
                {"nondeletable" , null},

            }
        },

        { 301,
          new Dictionary<string, object>()
            {
                {"name", "Pizza Voucher" },
                {"desc", "Voucher"},
                {"imag", "InventoryPics/pizzaRoundsTicket"},

                { "unusable", "Redeemable at participating Pizza Rounds® or Pizza Rounds Express® locations" },
                {"nonconsumable", null },
            }
        },


        { 900,
          new Dictionary<string, object>()
            {
                {"name", "Global Gains" },
                {"desc", "This pamphlet isn't afraid to tell the truth about global warming."},
                {"imag", "Literature/Pics/globalGains"},

                {"nonconsumable", null },
                {"literature", "Literature/globalGains" },
            }
        },

        { 901,
          new Dictionary<string, object>()
            {
                {"name", "Ordinal Procession" },
                {"desc", "When life gets too complex, here's how to get back on track."},
                {"imag", "Literature/Pics/ordinalProcession"},

                {"nonconsumable", null },
                {"literature", "Literature/ordinalProcession" },
            }
        },

        { 902,
          new Dictionary<string, object>()
            {
                {"name", "Code Guide" },
                {"desc", "This book is very, very thin."},
                {"imag", "Literature/Pics/codeGuide"},

                {"nonconsumable", null },
                {"literature", "Literature/codeGuide" },
            }
        },



    };

    public static readonly Dictionary<string, int[]> itemCategories = new Dictionary<string, int[]>()
    {
        {"all", allItems.Keys.ToArray()},
        {"normal food", allItems.Keys.Where(x => (x/100 == 2)).ToArray() },
    };

    public static float FloatMaker(object x)
    {
        try
        {
            return ((FakerandEvaluator<float>)x).Evaluate();
        }
        catch
        {
            try
            {
                return (float)x;
            }
            catch
            {
                return float.NaN;
            }
        }
    }

    public static ItemInfoHolder GetItemInfo(int i, bool trueForDirectIDFalseForSlotIndex = false)
    {
        if (!trueForDirectIDFalseForSlotIndex)
        {
            i = Utilities.loadedSaveData.SharedPlayerItems[i];
        }
        
        object n = null; object d = null; object im = null;
        if (allItems.ContainsKey(i))
        {
            Dictionary<string, object> itemInfo = allItems[i];
            itemInfo.TryGetValue("name", out n);
            itemInfo.TryGetValue("desc", out d);
            itemInfo.TryGetValue("imag", out im);
        }
        return new ItemInfoHolder(n, d, im);
    }

    public static void UseItem(int inventorySlot, GameObject plr, bool usedByPlayerVolition = true)
    {
        if (KHealth.someoneDied || Door1.levelComplete)
        {
            //Debug.Log("no use item after someone died, or door entered");
            return;
        }

        if (inventorySlot >= Utilities.loadedSaveData.SharedPlayerItems.Count || inventorySlot < 0)
        {
            //Debug.Log("bad inventory index");
            return;
        }

        KHealth kh = plr.GetComponent<KHealth>();
        BasicMove bm = plr.GetComponent<BasicMove>();
        SpecialGunTemplate cct = plr.GetComponent<SpecialGunTemplate>();

        int itemId = Utilities.loadedSaveData.SharedPlayerItems[inventorySlot];

        if (!allItems.ContainsKey(itemId))
        {
            //Debug.Log("item #" + itemId + " doesn't exist, so i can't use it");
            return;
        }

        Dictionary<string, object> itemInfo = allItems[itemId];

        if (itemInfo.ContainsKey("unusable") && usedByPlayerVolition)
        {
            dialogMessage = (string)itemInfo["unusable"];
            if (dialogMessage != "") { MakeDialog(plr); }
            return;
        }

        if (itemInfo.ContainsKey("heal") && kh)
        {
            kh.ChangeHealth(FloatMaker(itemInfo["heal"]), "");
        }

        if (itemInfo.ContainsKey("burn") && kh)
        {
            float burnFactor = FloatMaker(itemInfo["burn"]);
            if (burnFactor > 0f)
            {
                kh.overheat += burnFactor;
            }
        }

        if (itemInfo.ContainsKey("energy"))
        {
            if (cct)
            {
                cct.gunHealth += FloatMaker(itemInfo["energy"]);
            }
        }

        if (itemInfo.ContainsKey("slide roulette"))
        {
            GameObject ne = UnityEngine.Object.Instantiate(slideRoulettePrefab, Vector3.zero, Quaternion.identity);
            ne = ne.transform.Find("SlideRouletteBox").gameObject;
            ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
            ne.SetActive(true);
        }

        if (itemInfo.ContainsKey("jump power") && bm)
        {
            bm.jumpHeight += FloatMaker(itemInfo["jump power"]);
        }

        if (itemInfo.ContainsKey("move speed") && bm)
        {
            bm.moveSpeed += FloatMaker(itemInfo["move speed"]);
        }

        if (itemInfo.ContainsKey("tired high") && kh)
        {
            kh.tiredOrHigh = FloatMaker(itemInfo["tired high"]);
        }

        if (itemInfo.ContainsKey("melee power") && bm)
        {
            bm.punchPowerMultiplier += FloatMaker(itemInfo["melee power"]);
        }

        if (itemInfo.ContainsKey("size mul"))
        {
            float smul = FloatMaker(itemInfo["size mul"]);
            Vector3 oldSize = bm.transform.localScale;
            if (smul > 1f && Mathf.Abs(oldSize.x) > 1.95f && Mathf.Abs(oldSize.y) > 1.95f)
            {
                Debug.Log("...");
                dialogMessage = "Already big!";
                MakeDialog(plr);
                return;
            }
            if (smul < 1f && Mathf.Abs(oldSize.x) < 0.53f && Mathf.Abs(oldSize.y) < 0.53f)
            {
                dialogMessage = "Already small!";
                MakeDialog(plr);
                return;
            }

            bm.transform.localScale = new Vector3(oldSize.x * smul, oldSize.y * smul, oldSize.z);

            Vector3 fakeUp = bm.transform.up * ((bm.transform.localScale.y < 0) ? -1 : 1);
            float boxY = 18f;
            float currVSize = boxY * Mathf.Abs(bm.transform.localScale.y);
            float oldVSize = boxY * Mathf.Abs(oldSize.y);
            bm.transform.position += (currVSize - oldVSize) * fakeUp;

            dialogMessage = "Current relative scale is now X=" + bm.transform.localScale.x + " , Y=" + bm.transform.localScale.y + " .";
            MakeDialog(plr);
        }

        if (itemInfo.ContainsKey("used msg"))
        {
            dialogMessage = (string)itemInfo["used msg"];
            MakeDialog(plr);
        }

        if (itemInfo.ContainsKey("custom func"))
        {
            customFuncDeleteSkip = false;
            dialogMessage = ((CustomFunc)itemInfo["custom func"]).Evaluate();
            if (dialogMessage != "") { MakeDialog(plr); }
            if (customFuncDeleteSkip) { return; }
        }

        if (!itemInfo.ContainsKey("nonconsumable"))
        {
            ++Utilities.loadedSaveData.itemsUsed;
            ++Utilities.toSaveData.itemsUsed;
            Utilities.loadedSaveData.SharedPlayerItems.RemoveAt(inventorySlot);
            if (!eatEffect) { eatEffect = Resources.Load<GameObject>("EatEffect"); }
            GameObject.Instantiate(eatEffect, bm.transform.position, bm.transform.rotation);
        }
        else
        {
            Debug.Log("didn't delete nonconsumable object");
        }

        return;
    }

    public static bool InventoryFullHandling(bool quiet = false, GameObject mainSpeaker = null)
    {
        bool d = Utilities.loadedSaveData.SharedPlayerItems.Count >= Utilities.getInventorySlotCount();
        if (d && !quiet)
        {
            dialogMessage = fullInventoryMessage;
            MakeDialog(mainSpeaker);
        }
        return d;
    }

    public static bool AddItem(int itemId, bool ignoreCount = false)
    {
        if (Utilities.loadedSaveData.SharedPlayerItems.Count < Utilities.getInventorySlotCount() || ignoreCount)
        {
            Utilities.loadedSaveData.SharedPlayerItems.Insert(0, itemId);
            return true;
        }
        return false;
    }

    public static void DeleteItemByIndex(int inventorySlot)
    {
        try
        {
            //int a = Utilities.loadedSaveData.SharedPlayerItems[inventorySlot];
            //if (!allItems[Utilities.loadedSaveData.SharedPlayerItems[inventorySlot]].ContainsKey("nondeletable"))
            {
                Utilities.loadedSaveData.SharedPlayerItems.RemoveAt(inventorySlot);
            }
        }
        catch
        {
            Debug.Log(inventorySlot + " is a bad inventory slot index");
        }
    }

    public static bool DeleteItemById(int id, bool deleteOnlyOne = true)
    {
        bool deleteSucc = false;
        for (int i = 0; i < Utilities.loadedSaveData.SharedPlayerItems.Count; ++i)
        {
            if (Utilities.loadedSaveData.SharedPlayerItems[i] == id)
            {
                Utilities.loadedSaveData.SharedPlayerItems.RemoveAt(i);
                deleteSucc = true;
                if (deleteOnlyOne) { return true; }
            }
        }
        return deleteSucc;
    }
}

//old inventory sprites
[System.Obsolete("Old", true)]
public static class InventoryItemsClass { 

    [System.Serializable]
    public class ItemData
    {
        public string name;
        public Sprite image;
        public string descriptionText;
        public bool isFood;
        public bool isEquippable;
        public string onConsumedText;
        public float healthRegen;
        public float energyRegen;
        public string[] categories;
        public Dictionary<string, string> specialActions;
        public int isDeletable;
        public string onDeleteText;
        public GameObject relatedPrefab;

        public ItemData(string _name, Sprite _image, string _desc, bool _isFood, bool _isEquippable, string _onConsumedTxt, float _hRegen, float _eRegen, string[] _categories, Dictionary<string, string> _specialActions, int _isDeletable, string _onDeleteTxt, GameObject _relatedPrefab)
        {
            name = _name; image = _image; descriptionText = _desc;
            isFood = _isFood;
            isEquippable = _isEquippable;
            onConsumedText = _onConsumedTxt;
            healthRegen = _hRegen;
            energyRegen = _eRegen;
            categories = _categories;
            specialActions = _specialActions;
            isDeletable = _isDeletable;
            onDeleteText = _onDeleteTxt;
            relatedPrefab = _relatedPrefab;
        }

        public ItemData(string _name, Sprite _image, string _desc, bool _isEquippable, string _onConsumedTxt, Dictionary<string, string> _specialActions, int _isDeletable, string _onDeleteTxt, GameObject _relatedPrefab)
        {
            name = _name; image = _image; descriptionText = _desc; //stuff that is not food
            isEquippable = _isEquippable;
            onConsumedText = _onConsumedTxt;
            specialActions = _specialActions;
            isDeletable = _isDeletable;
            onDeleteText = _onDeleteTxt;
            relatedPrefab = _relatedPrefab;
        }
    }
}
