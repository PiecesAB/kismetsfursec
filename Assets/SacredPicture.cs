using UnityEngine;
using System.Collections;

public class SacredPicture : MonoBehaviour {

    /*public Texture2D compareSprite;
    public RenderTexture ab;*/
    public Sprite normiePicture;
    public Sprite wokePicture;
    public GameObject bulletprefab;
    public Sprite bulletsprite;
    public float bulletdamagemult;
    public float bulletspeed;
    public float angleoffset = -90;
    public GameObject currentplayer;
    public Vector3 lefteye;
    public Vector3 righteye;
    private Color gray = new Color(0.75f, 0.75f, 0.75f);

    public void Fire()
    {
        Vector3 di = -transform.position - lefteye + currentplayer.transform.position;
        float ang = Mathf.Atan2(di.y,di.x)*Mathf.Rad2Deg + angleoffset;
        GameObject nb = (GameObject)Instantiate(bulletprefab, transform.position + lefteye, Quaternion.AngleAxis(ang, Vector3.forward));

        di = -transform.position - righteye + currentplayer.transform.position;
        ang = Mathf.Atan2(di.y, di.x) * Mathf.Rad2Deg + angleoffset;
        GameObject nb2 = (GameObject)Instantiate(bulletprefab, transform.position + righteye, Quaternion.AngleAxis(ang, Vector3.forward));


        nb.GetComponent<NormalBulletBehavior>().damageMultiplier = nb2.GetComponent<NormalBulletBehavior>().damageMultiplier = bulletdamagemult;
        nb.GetComponent<NormalBulletBehavior>().speed = nb2.GetComponent<NormalBulletBehavior>().speed = bulletspeed;
        nb.GetComponent<SpriteRenderer>().sprite = nb.GetComponent<SpriteRenderer>().sprite = bulletsprite;
    }

	void Start () {
        /*ab = new RenderTexture(36, 48, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        ab.Create();
        GetComponent<Camera>().targetTexture = ab;*/
	}
	
	void Update () {
        /* RenderTexture.active = ab;
         Texture2D temp = new Texture2D(36, 48, TextureFormat.ARGB32,false,false);
         temp.ReadPixels(new Rect(0, 0, 36, 48), 0, 0);
         temp.Apply();
         if (temp.GetPixels32().Equals(compareSprite.GetPixels32()))
             {
                 print("obstructed");
             }*/
        if (Time.timeScale > 0)
        {
            if (((System.Math.Abs(Camera.main.transform.position.x - transform.position.x) > 142 || System.Math.Abs(Camera.main.transform.position.y - transform.position.y) > 84) && GetComponent<Renderer>().isVisible) || Physics2D.BoxCast(transform.position, new Vector2(36, 48), 0, Vector2.up, 0, int.MaxValue, -100000, 100000))
            {
                GetComponent<SpriteRenderer>().sprite = wokePicture;
                GetComponent<SpriteRenderer>().color = Color.white;
                Fire();
            }
            else
            {
                GetComponent<SpriteRenderer>().color = gray;
                GetComponent<SpriteRenderer>().sprite = normiePicture;
            }
        }
	}
}
