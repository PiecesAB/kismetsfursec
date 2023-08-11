using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerExclusionBlock : MonoBehaviour
{
    public enum PlayerName
    {
        Khal, Tetra, Myst, Ravel, None
    }

    public PlayerName playerName;
    public Sprite[] onSprites;
    public Sprite[] offSprites;
    public bool inverted = false;

    private BoxCollider2D col;
    private SpriteRenderer sr;

    [HideInInspector]
    public static PlayerName currPlayerName = PlayerName.None;

    private PlayerName lastPlayerName = PlayerName.None;

    private static PlayerExclusionBlock main;

    public static void UpdatePlayerName(PrimPlayableCharacter plr)
    {
        if (plr == null) { return; }
        try { currPlayerName = (PlayerName)Enum.Parse(typeof(PlayerName), plr.myName); }
        catch { print("player for player exclusion block does not exist."); }
    }

    private void LateUpdate()
    {
        if (lastPlayerName != currPlayerName)
        {
            UpdateImage();
            lastPlayerName = currPlayerName;
        }
    }

    private void UpdateImage()
    {
        if ((int)playerName >= onSprites.Length) { return; }

        if (!Application.isPlaying)
        {
            sr.sprite = onSprites[(int)playerName];
            return;
        }

        bool on = (currPlayerName == playerName);
        if (inverted) { on = !on; }
        col.enabled = on;
        sr.sprite = on ? onSprites[(int)playerName] : offSprites[(int)playerName];
    }

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (!Application.isPlaying) { UpdateImage(); }
        UpdateImage();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateImage();
        }
    }
#endif
}
