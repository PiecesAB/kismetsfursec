using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleBlock : MonoBehaviour {

    public static Vector4[] positions = new Vector4[4];
    public static List<Transform> players = new List<Transform>();
    [HideInInspector]
    public SpriteRenderer rend;
    public Renderer visRend;

    private void PopulatePlayers()
    {
        if (players.Count == 0 || players[0] == null)
        {
            Encontrolmentation[] plrsg = LevelInfoContainer.GetNonNullPlayerList();
            positions[0] = positions[1] = positions[2] = positions[3] = Vector4.zero;
            players.Clear();
            for (int i = 0; i < Mathf.Min(plrsg.Length, 4); i++)
            {
                if (plrsg[i] == null) { continue; }
                players.Add(plrsg[i].transform);
            }
        }
    }

    void Start () {
        PopulatePlayers();
        rend = GetComponent<SpriteRenderer>();
        rend.color = Color.clear;
	}
	
	void Update () {

        if (Time.timeScale == 0) { return; }
        if (!visRend.isVisible) { return; }
        PopulatePlayers();

        for (int i = 0; i < 4; i++)
        {
            if (i < players.Count)
            {
                positions[i] = (Vector2)players[i].position;
            }
            else
            {
                positions[i] = Vector4.zero;
            }
        }
        rend.material.SetVectorArray(Shader.PropertyToID("_PlayerPositions"), positions);
        rend.color = Color.white;
    }
}
