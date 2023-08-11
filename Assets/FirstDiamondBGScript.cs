using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FirstDiamondBGScript : MonoBehaviour {

    private List<GameObject> tiles = new List<GameObject>();
    public GameObject diamondTile;
    public Color diamondColor;

    // Use this for initialization
    void Start () {
        for (int yPos = 320; yPos >= -320; yPos -= 40)
        {
            for (int xPos = -400 + (yPos % 80); xPos <= 400 - (yPos % 80); xPos += 80)
            {
                GameObject newTile = (GameObject)Instantiate(diamondTile, new Vector3(xPos, yPos), Quaternion.identity);
                newTile.transform.SetParent(transform, false);
                newTile.GetComponent<Image>().color = diamondColor;
                newTile.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                tiles.Add(newTile);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        foreach (GameObject tile in tiles)
        {
            tile.transform.localPosition = tile.transform.localPosition + new Vector3((3*Mathf.PerlinNoise((float)DoubleTime.UnscaledTimeRunning/5,0))-1, (3*Mathf.PerlinNoise(0, (float)DoubleTime.UnscaledTimeRunning/5))-1, 0);
            if (tile.transform.localPosition.y < -320)
            {
                tile.transform.localPosition = new Vector3(tile.transform.localPosition.x, 320, 0);
            }
            if (tile.transform.localPosition.y > 320)
            {
                tile.transform.localPosition = new Vector3(tile.transform.localPosition.x, -320, 0);
            }
            if (tile.transform.localPosition.x > 400)
            {
                tile.transform.localPosition = new Vector3(-400, tile.transform.localPosition.y, 0);
            }
            if (tile.transform.localPosition.x < -400)
            {
                tile.transform.localPosition = new Vector3(400, tile.transform.localPosition.y, 0);
            }
        }
	}
}
