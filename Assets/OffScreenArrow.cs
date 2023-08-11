using UnityEngine;
using System.Collections;

public class OffScreenArrow : MonoBehaviour {

    public Camera cam;
    public SpriteRenderer hud;
    public Sprite up;
    public Sprite diagonal;
    public Sprite right;
    public TextMesh textMesh;

	// Update is called once per frame
	void Update () {
        if (!GetComponent<Renderer>().isVisible && GetComponent<BasicMove>() != null)
        {
            if (transform.localScale.magnitude == 0) { return; }
            hud.transform.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);
            hud.color = Color.white;

            Vector2 max = new Vector2(cam.transform.position.x+(cam.orthographicSize*cam.aspect), cam.transform.position.y + cam.orthographicSize);
            Vector2 min = new Vector2(cam.transform.position.x - (cam.orthographicSize * cam.aspect), cam.transform.position.y - cam.orthographicSize);
            hud.gameObject.transform.position = new Vector2(Mathf.Clamp(transform.position.x,min.x+8,max.x-8), Mathf.Clamp(transform.position.y,min.y+8,max.y-8));
            hud.gameObject.transform.localEulerAngles = -transform.eulerAngles; // hack for upside-down plr... what happens if sideways?

            //choose the right direction to point
            if (System.Math.Abs(hud.gameObject.transform.position.x-(min.x+8f)) < 1f)
            {
                hud.flipX = true;
                if (System.Math.Abs(hud.gameObject.transform.position.y - (min.y + 8f)) < 1f)
                {
                    //bottomleft
                    hud.sprite = diagonal;
                    
                    hud.flipY = true;
                }
                else if (System.Math.Abs(hud.gameObject.transform.position.y - (max.y - 8f)) < 1f)
                {
                    //topleft
                    hud.sprite = diagonal;
                    hud.flipY = false;
                }
                else
                {
                    //left
                    hud.sprite = right;
                    hud.flipY = false;
                }
            }
            else if (System.Math.Abs(hud.gameObject.transform.position.x - (max.x - 8f)) < 1f)
            {
                hud.flipX = false;
                if (System.Math.Abs(hud.gameObject.transform.position.y - (min.y + 8f)) < 1f)
                {
                    //bottomright
                    hud.sprite = diagonal;
                    hud.flipY = true;
                }
                else if (System.Math.Abs(hud.gameObject.transform.position.y - (max.y - 8f)) < 1f)
                {
                    //topright
                    hud.sprite = diagonal;
                    hud.flipY = false;
                }
                else
                {
                    //right
                    hud.sprite = right;
                    hud.flipY = false;
                }
            }
            else
            {
                hud.flipX = false;
                if (System.Math.Abs(hud.gameObject.transform.position.y - (min.y + 8f)) < 1f)
                {
                    //bottom
                    hud.sprite = up;
                    hud.flipY = true;
                }
                else if (System.Math.Abs(hud.gameObject.transform.position.y - (max.y - 8f)) < 1f)
                {
                    //top
                    hud.sprite = up;
                    hud.flipY = false;
                }
                else
                {
                    //WHAT
                }
            }

            textMesh.color = Color.white;
            float d = Fastmath.FastV2Dist(new Vector2(Mathf.Clamp(transform.position.x, min.x, max.x), Mathf.Clamp(transform.position.y, min.y, max.y)), gameObject.transform.position);
            //distance in pixels
            d *= 0.0508f; //distance now in meters
            if (d < 10)
            {
                textMesh.text = d.ToString("0.0");
            }
            else
            {
                d = Mathf.Min(d, 99f);
                textMesh.text = d.ToString("##");
            }
        }
        else
        {
            hud.color = Color.clear;
            textMesh.color = Color.clear;
        }


	}
}
