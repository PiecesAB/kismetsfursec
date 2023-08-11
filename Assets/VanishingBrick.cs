using UnityEngine;
using System.Collections;

public class VanishingBrick : MonoBehaviour {

    private bool isVanishing = false;

    public IEnumerator Vanish()
    {
        if (!isVanishing)
        {
            isVanishing = true;
            Color c = GetComponent<SpriteRenderer>().color;
            for (float i = c.a; i > 0f; i -= 0.03f)
            {
                GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, Mathf.Clamp01(i));
                yield return new WaitForEndOfFrame();
            }
            GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 0f);
            GetComponent<Collider2D>().enabled = false;
            yield return new WaitForSeconds(3.0f);
            isVanishing = false;
            GetComponent<Collider2D>().enabled = true;
            for (float i = 0f; i < 1f; i += 0.05f)
            {
                GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, Mathf.Clamp01(i));
                yield return new WaitForEndOfFrame();
            }
            GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            print("hi");
            if (!isVanishing)
            {
                StopAllCoroutines();
            }
            StartCoroutine(Vanish());

        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
