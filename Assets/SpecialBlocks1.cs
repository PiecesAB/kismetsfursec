using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Obsolete("Old", true)]
public class SpecialBlocks1 : GenericBlowMeUp {

    // why were these not separate classes before?
    // well they are now.
    // this is obsolete.

    public enum SpecialBlock1Type
    {
        Mine, Glue, Fade, InstantDeath, Ice, TouchLimit, Heat
    }

    public SpecialBlock1Type type;
    [Header("v1 is explosion for mines but also other crap")]
    public float v1=1800f;
    [Header("d is the damage if applicable")]
    public float d;
    [Header("e is the asplosion object, let's say it's bullets")]
    public GameObject e;
    [Header("em is the asplosion material (outdated)")]
    public Material em;
    [Header("w is the fade wait time")]
    public float w;
    [Header("does this make a sound")]
    public AudioSource soundObj;
    [Header("for fade block physical collider")]
    public Collider2D cd;
    [Header("the render cube for screen effects...")]
    public MeshRenderer rc;
    [Header("[insert needed materials here]")]
    public Material[] mats;
    [Header("i don't know lol. it depends what type")]
    public GameObject[] lol;
    [Header("like lol but for music")]
    public AudioSource[] moreaudio;
    [Header("For flammable objects")]
    public float timeLeft = -1f;
    [Header("my main renderer (for mine)")]
    public Renderer myMainRenderer = null;
    [Header("also for flammable")]
    public BoxCollider2D[] boxFlameCollidersToCopy;
    public PolygonCollider2D[] polygonFlameCollidersToCopy;
    public GameObject flameToCopy;
    public GameObject dustBlockWhenBurnOver;

    private double trs;
    private Shader mms;
    private bool fading;
    private bool toush = false;
    private bool toushB = false;
    private int int1 = 0;
    private int cooldown = 0;
    private List<KHealth> healths = new List<KHealth>();
    private List<GameObject> colObjs = new List<GameObject>();

    private const float fireDelay = 0.5f;
    private const float burnTime = 8f;

    private static bool dyingInstantly = false;

	
}
