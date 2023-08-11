using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;
using System;

public class DifferentWindowTest : MonoBehaviour {

    public bool block = false;

    private bool opened = false;

    private int updateInterval = 60;

    private GameObject plr;

	private void Start () {
        if (!block)
        {
            var procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            int p = procs.Length;
            if (p > 2)
            {
                Application.Quit();
            }
            else if (p == 2)
            {
                Utilities.thisIsTheSecondWindow = true;
                SceneManager.LoadSceneAsync("AlternateWindowBonus");
            }
            else //1
            {
                PlayerPrefs.SetInt("NumberOfWindows", 1);
                PlayerPrefs.Save();
            }
        }
        else //for the special level
        {
            plr = FindObjectOfType<BasicMove>().gameObject;
            opened = false;
            if (Utilities.thisIsTheSecondWindow)
            {
                PlayerPrefs.SetInt("NumberOfWindows", 2);
                PlayerPrefs.Save();
                Destroy(GameObject.Find("Guis Only"));
                Destroy(FindObjectOfType<OffScreenArrow>());
                //Destroy(plr.GetComponent<BasicMove>());
                plr.GetComponent<Encontrolmentation>().allowUserInput = false;
                //Destroy(plr.GetComponent<Rigidbody2D>());
                //foreach (Collider2D c in plr.GetComponents<Collider2D>())
                //{
                //    Destroy(c);
                //}
                //Destroy(plr.GetComponent<KHealth>());
                Destroy(plr.GetComponent<ClickToChangeTime>());
                Camera.main.transform.position += Vector3.left * 320f;
            }
        }
	}

    private void OnApplicationQuit()
    {
            PlayerPrefs.SetInt("NumberOfWindows", 1);
            PlayerPrefs.Save();
    }

    private void Update () {
        int p = PlayerPrefs.GetInt("NumberOfWindows", 1);
        if (block)
        {
            if (p >= 2 && GetComponent<Collider2D>()) //&& !Utilities.thisIsTheSecondWindow)
            {
                GetComponent<Collider2D>().enabled = false;
                opened = true;
            }

            if (plr && plr.GetComponent<Encontrolmentation>() && !(KHealth.someoneDied || Door1.levelComplete) && Time.timeScale > 0)
            {
                if (!Utilities.thisIsTheSecondWindow)
                {
                    PlayerPrefs.SetFloat("AlternateWindowPlrXPos", plr.transform.position.x);
                    PlayerPrefs.SetFloat("AlternateWindowPlrYPos", plr.transform.position.y);
                    PlayerPrefs.SetString("AlternateWindowPlrMvt", plr.GetComponent<Encontrolmentation>().currentState.ToString());
                    PlayerPrefs.Save();
                }
                else
                {
                    plr.transform.position = new Vector3(
                        PlayerPrefs.GetFloat("AlternateWindowPlrXPos", 0),
                        PlayerPrefs.GetFloat("AlternateWindowPlrYPos", 0),
                        plr.transform.position.z);
                    plr.GetComponent<Encontrolmentation>().fakeInput = System.Convert.ToUInt64(PlayerPrefs.GetString("AlternateWindowPlrMvt", "0"));
                }
            }

            if (p == 1 && Utilities.thisIsTheSecondWindow) //don't want a second window by itself
            {
                Application.Quit();
            }

            if (p == 1 && opened && !Door1.levelComplete)
            {
                if (plr && plr.transform.position.x < transform.position.x + 5f)
                {
                    plr.GetComponent<KHealth>().ChangeHealth(Mathf.NegativeInfinity, "window close");
                }
                opened = false;
                GetComponent<Collider2D>().enabled = true;
            }

        }


        if (Door1.levelComplete || KHealth.someoneDied)
            {
                PlayerPrefs.SetInt("NumberOfWindows", 1);
                PlayerPrefs.Save();
            }
	}
}
