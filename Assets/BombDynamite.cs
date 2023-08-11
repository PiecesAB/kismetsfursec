using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDynamite : GenericBomb
{
    public SpriteRenderer renderSquare;
    public GameObject warningCircle;
    public MeshRenderer bombModel;
    public SpriteRenderer screenSprite;
    public Collider2D myCollider;
    public Collider2D enemyDamageCollider;
    public GameObject explosionBullets;
    private bool db;

    public AudioClip BOOM;

    private AudioSource sound;
    private SpriteRenderer newCircle;

    protected override void SubStart()
    {
        db = false;
        sound = GetComponent<AudioSource>();
    }

    protected override void SubUpdate()
    {
        if (!db && renderSquare.isVisible)
        {
            db = true;
            Activate();
        }

        if (newCircle != null)
        {
            newCircle.color = new Color(newCircle.color.r, newCircle.color.g, newCircle.color.b, newCircle.color.a - 0.03333333f * Time.timeScale);
            if (newCircle.color.a < 0.01f) { Destroy(newCircle); newCircle = null; }
        }
    }

    protected override bool FinalClearanceBeforeExplode()
    {
        return renderSquare.isVisible && Time.timeScale > 0;
    }

    public override void OnStartCountdown()
    {
        newCircle = (Instantiate(warningCircle, transform.position, Quaternion.identity) as GameObject).GetComponent<SpriteRenderer>();
    }

    public override IEnumerator Explode()
    {
        GetComponent<GenericBlowMeUp>().BlowMeUp(3f);
        Destroy(bombModel); Destroy(screenSprite); Destroy(myCollider); Destroy(enemyDamageCollider);
        sound.Stop();
        sound.clip = BOOM;
        sound.volume = volo;
        sound.Play();
        GameObject bullets = Instantiate(explosionBullets, transform.position, transform.rotation);
        bullets.SetActive(true);
        yield return null;
    }
}
