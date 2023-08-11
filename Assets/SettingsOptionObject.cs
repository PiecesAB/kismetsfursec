using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class SettingsOptionObject : MonoBehaviour {

    public enum PrefType
    {
        Float,Int,String
    }

    public enum SettingsToUpdate
    {
        None, Sound, UI, Players
    }

    public string PenName;
    public string RealName;
    public InSaveMenuBase secondaryInterface;
    public MenuScrollHandler menuScrollHandler;
    public SettingsInteractables.Type interfaceType;
    public GameObject[] interactables;
    public PrefType DataType;
    //public float DefaultFloat;
    //public int DefaultInt;
    //public string DefaultString;
    public float LoadedFloat;
    public int LoadedInt;
    public string LoadedString;
    public Vector3 minMaxInc;
    public string[] choices;
    public static int currentMenuChoice;
    public SettingsToUpdate settingsToUpdate = SettingsToUpdate.None;


    public Text title;
    public int menuPosition;
    public GameObject boxToPutTheInterfaceIn;
    public GameObject MyInterface;

    void UpdateSettings()
    {
        if (RealName == "") { return; }

        object uo = Settings.data[RealName];
        if (uo is string) { Settings.data[RealName] = LoadedString; }
        else if (uo is int) { Settings.data[RealName] = LoadedInt; }
        else if (uo is float) { Settings.data[RealName] = LoadedFloat; }

        switch (settingsToUpdate)
        {
            case SettingsToUpdate.Sound:
                Settings.UpdateSound(); break;
            case SettingsToUpdate.UI:
                Settings.UpdateUI(); break;
            case SettingsToUpdate.Players:
                Settings.UpdatePlayers(); break;
            default:
            case SettingsToUpdate.None:
                break;
        }
    }

    void Start () {
        currentMenuChoice = 0;
        if (DataType == PrefType.Float)
        {
            LoadedFloat = PlayerPrefs.GetFloat(RealName, (float)(Settings.data?[RealName] ?? 0f));
            PlayerPrefs.SetFloat(RealName, LoadedFloat);
        }
        if (DataType == PrefType.Int)
        {
            if (interfaceType == SettingsInteractables.Type.Button || interfaceType == SettingsInteractables.Type.SuperButton)
            {
                LoadedInt = 0;
                if (RealName != "")
                {
                    PlayerPrefs.SetInt(RealName, 0);
                }
            }
            else
            {
                LoadedInt = PlayerPrefs.GetInt(RealName, (int)(Settings.data?[RealName] ?? 0));
                PlayerPrefs.SetInt(RealName, LoadedInt);
            }
        }
        if (DataType == PrefType.String)
        {
            LoadedString = PlayerPrefs.GetString(RealName, (string)(Settings.data?[RealName] ?? ""));
            PlayerPrefs.SetString(RealName, LoadedString);
        }
        PlayerPrefs.Save();

        if (interfaceType == SettingsInteractables.Type.Button)
        {
            //only int is possible
            if (DataType != PrefType.Int)
            {
                Debug.LogError("Use only int in a button you moron.");
            }
            MyInterface = Instantiate(interactables[3]);
            MyInterface.transform.SetParent(boxToPutTheInterfaceIn.transform);
            MyInterface.transform.localPosition = Vector3.zero;
            MyInterface.transform.localRotation = Quaternion.identity;
            MyInterface.transform.localScale = Vector3.one;
            if (menuPosition == menuScrollHandler.index)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
            else
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }
            MyInterface.GetComponent<SettingsInteractables>().secondaryInterface = secondaryInterface;
        }

        if (interfaceType == SettingsInteractables.Type.Switch)
        {
            //only int is possible
            if (DataType != PrefType.Int)
            {
                Debug.LogError("Use only int in a switch you moron.");
            }
            MyInterface = Instantiate(interactables[1]);
            MyInterface.transform.SetParent(boxToPutTheInterfaceIn.transform);
            MyInterface.transform.localPosition = Vector3.zero;
            MyInterface.transform.localRotation = Quaternion.identity;
            MyInterface.transform.localScale = Vector3.one;
            if (menuPosition == menuScrollHandler.index)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
            else
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }

            MyInterface.GetComponent<SettingsInteractables>().myOwnValue = LoadedInt * 3f;
            MyInterface.GetComponent<SettingsInteractables>().returnValue = LoadedInt;

        }

        if (interfaceType == SettingsInteractables.Type.Slider)
        {
            //only int is possible
            if (DataType != PrefType.Float)
            {
                Debug.LogError("Use only float in a slider you moron.");
            }
            MyInterface = Instantiate(interactables[0]);
            MyInterface.transform.SetParent(boxToPutTheInterfaceIn.transform);
            MyInterface.transform.localPosition = Vector3.zero;
            MyInterface.transform.localRotation = Quaternion.identity;
            MyInterface.transform.localScale = Vector3.one;
            if (menuPosition == menuScrollHandler.index)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
            else
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }

            MyInterface.GetComponent<SettingsInteractables>().min = minMaxInc.x;
            MyInterface.GetComponent<SettingsInteractables>().max = minMaxInc.y;
            MyInterface.GetComponent<SettingsInteractables>().increment = minMaxInc.z;
            MyInterface.GetComponent<SettingsInteractables>().myOwnValue = LoadedFloat;

        }

        if (interfaceType == SettingsInteractables.Type.Choice)
        {
            //only int is possible
            if (DataType != PrefType.String)
            {
                Debug.LogError("Use only string in a choicebox.");
            }
            MyInterface = Instantiate(interactables[2]);
            MyInterface.transform.SetParent(boxToPutTheInterfaceIn.transform);
            MyInterface.transform.localPosition = Vector3.zero;
            MyInterface.transform.localRotation = Quaternion.identity;
            MyInterface.transform.localScale = Vector3.one;
            if (menuPosition == menuScrollHandler.index)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
            else
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }

            MyInterface.GetComponent<SettingsInteractables>().choices = choices;
            LoadedInt = choices.ToList().IndexOf(LoadedString);
            MyInterface.GetComponent<SettingsInteractables>().index = LoadedInt;
            MyInterface.GetComponent<SettingsInteractables>().myOwnValue = 1f;

        }

        if (interfaceType == SettingsInteractables.Type.SuperButton)
        {
            //only int is possible
            if (DataType != PrefType.Int)
            {
                Debug.LogError("Use only int in a super-button.");
            }
            MyInterface = Instantiate(interactables[4]);
            MyInterface.transform.SetParent(boxToPutTheInterfaceIn.transform);
            MyInterface.transform.localPosition = Vector3.zero;
            MyInterface.transform.localRotation = Quaternion.identity;
            MyInterface.transform.localScale = Vector3.one;
            if (menuPosition == menuScrollHandler.index)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
            else
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }
        }
        title.text = PenName;
    }
	
	void Update () {
        if (interfaceType == SettingsInteractables.Type.Button || interfaceType == SettingsInteractables.Type.SuperButton)
        {
            //only int is possible
            if (DataType != PrefType.Int)
            {
                Debug.LogError("Use only int in a button.");
            }
            if (MyInterface.GetComponent<SettingsInteractables>().returnValue == 1f && LoadedInt != 1)
            {
                LoadedInt = 1;
                if (RealName != "")
                {
                    PlayerPrefs.SetInt(RealName, 1);
                }
                PlayerPrefs.Save();
                UpdateSettings();
            }
        }

        if (interfaceType == SettingsInteractables.Type.Switch)
        {
            //only int is possible
            if (DataType != PrefType.Int)
            {
                Debug.LogError("Use only int in a switch.");
            }
            if (MyInterface.GetComponent<SettingsInteractables>().returnValue == 1f && LoadedInt != 1)
            {
                LoadedInt = 1;
                PlayerPrefs.SetInt(RealName, 1);
                UpdateSettings();
            }
            if (MyInterface.GetComponent<SettingsInteractables>().returnValue == 0f && LoadedInt != 0)
            {
                LoadedInt = 0;
                PlayerPrefs.SetInt(RealName, 0);
                UpdateSettings();
            }
            PlayerPrefs.Save();
        }

        if (interfaceType == SettingsInteractables.Type.Slider)
        {
            //only int is possible
            if (DataType != PrefType.Float)
            {
                Debug.LogError("Use only float in a slider.");
            }
            if (MyInterface.GetComponent<SettingsInteractables>().myOwnValue != LoadedFloat)
            {
                LoadedFloat = MyInterface.GetComponent<SettingsInteractables>().myOwnValue;
                PlayerPrefs.SetFloat(RealName, LoadedFloat);
                UpdateSettings();
            }
            //add Save() as the player leaves settings!
            //never mind it's fine
        }

        if (interfaceType == SettingsInteractables.Type.Choice)
        {
            //only int is possible
            if (DataType != PrefType.String)
            {
                Debug.LogError("Use only string in a choicebox.");
            }
            if (MyInterface.GetComponent<SettingsInteractables>().index != LoadedInt)
            {
                LoadedInt = MyInterface.GetComponent<SettingsInteractables>().index;
                LoadedString = choices[LoadedInt];
                PlayerPrefs.SetString(RealName, LoadedString);
                UpdateSettings();
            }
            //add Save() as the player leaves settings!
            //never mind it's fine
        }

        
            if (menuPosition == menuScrollHandler.index)
            {
            if (MyInterface != null)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 0;
            }
                title.color = new Color(1f, 1f, 1f);
            }
            else
            {
            if (MyInterface != null)
            {
                MyInterface.GetComponent<SettingsInteractables>().disabled = 1;
            }
            title.color = new Color(0.125f, 0.125f, 0.125f);
            }
       
    }
}
