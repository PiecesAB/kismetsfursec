using UnityEngine;
using System.Collections;

public class PlayerWound : MonoBehaviour {

    public Sprite[] sprites;

    public void Start()
    {
        if (GetComponent<SpriteRenderer>())
        {
            GetComponent<SpriteRenderer>().sprite = sprites[Fakerand.Int(0, sprites.Length)];
            transform.localPosition = new Vector3(Mathf.Round(transform.localPosition.x), Mathf.Round(transform.localPosition.y), transform.localPosition.z);
        }
    }
}
