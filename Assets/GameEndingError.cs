using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameEndingError : MonoBehaviour
{
    private static string message;
    private static string waveText;
    private Encontrolmentation encmt;
    public Text textObj;
    public GameObject flashText;
    private int timer;
    public Text wave;

    public static void Throw(string msg)
    {
        message = msg;
        waveText = "Player wave: " + BrainwaveReader.GetFullString();
        SceneManager.LoadScene("FatalError");
    }

    void Start()
    {
        encmt = GetComponent<Encontrolmentation>();
        textObj.text = message + "\n" + Guid.NewGuid().ToString().ToUpper();
        wave.text = waveText;
        timer = 180;
        flashText.SetActive(false);
    }

    void Update()
    {
        if (encmt.AnyButtonDown()) { Application.Quit(1); }
        if (timer > 0) { --timer; }
        else { flashText.SetActive(true); }
    }
}
