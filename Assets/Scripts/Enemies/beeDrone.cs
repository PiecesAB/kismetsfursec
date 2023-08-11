using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beeDrone : GenericBlowMeUp
{
    public MeshRenderer myRenderer;
    public Material[] myColors;

    //target the active player, but also keeps distance from other drones
    private static List<beeDrone> allBees = new List<beeDrone>();
    private static List<beeDrone> onScreenBees = new List<beeDrone>();
    private static GameObject[] plrs;

    private double targetChangeTimer;
    private Vector2 moveDir;

    private Rigidbody2D rg2;

    private const float speedMult = 0.011111111f;
    private const float evadeOthersMult = 0.0002f;

    private float randWait;

    private bool wasEverOnScreen = false;

    private bool RendCheck()
    {
        if (MapViewerUI.main && MapViewerUI.main.viewing) { return false; }
        if (myRenderer.isVisible)
        {
            if (!onScreenBees.Contains(this))
            {
                onScreenBees.Add(this);
            }
            return true;
        }
        else
        {
            onScreenBees.Remove(this);
            return false;
        }
    }

    public override void Awake()
    {
        /*if (genericExplosionResource == null) //this is from the generic blow me up script
        {
            genericExplosionResource = Resources.Load<GameObject>("SmallExplo");
        }*/
        if (allBees.Count > 0)
        {
            allBees.Clear();
        }
        if (onScreenBees.Count > 0)
        {
            onScreenBees.Clear();
        }
        plrs = null;
    }

    private void Start()
    {
        allBees.Add(this);
        if (plrs == null)
        {
            plrs = GameObject.FindGameObjectsWithTag("Player");
        }
        randWait = Fakerand.Single() * 0.5f;
        targetChangeTimer = DoubleTime.ScaledTimeSinceLoad + randWait;
        rg2 = GetComponent<Rigidbody2D>();
        myRenderer.material = myColors[Fakerand.Int(0, myColors.Length)];

        RendCheck();
    }

    private void OnDestroy()
    {
        allBees.Remove(this);
        onScreenBees.Remove(this);
    }

    private void FixedUpdate()
    {
        if (rg2)
        {
            if (wasEverOnScreen)
            {
                rg2.velocity = moveDir * 60f;
            }
            else
            {
                rg2.velocity = Vector2.zero;
            }
        }
    }

    private void Update()
    {
        wasEverOnScreen = wasEverOnScreen || RendCheck();
        if (Time.timeScale > 0 && wasEverOnScreen)
        {
            if (DoubleTime.ScaledTimeSinceLoad > targetChangeTimer)
            {
                Vector2 plrPos = transform.position;
                for (int i = 0; i < plrs.Length; i++)
                {
                    if (plrs[i])
                    {
                        Encontrolmentation e = plrs[i].GetComponent<Encontrolmentation>();
                        if (e && e.allowUserInput && plrs[i].activeInHierarchy)
                        {
                            plrPos = plrs[i].transform.position;
                        }
                    }
                }
                moveDir = (plrPos - (Vector2)transform.position) * speedMult;
                if (moveDir != Vector2.zero)
                {
                    myRenderer.transform.eulerAngles = new Vector3(0, 0, Fastmath.FastAtan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg);

                    for (int j = 0; j < onScreenBees.Count; j++)
                    {
                        beeDrone bd = onScreenBees[j];
                        if (bd != this)
                        {
                            Vector2 vm = bd.transform.position - transform.position;
                            float sd = vm.sqrMagnitude;
                            if (sd < 2304f) //48 squared
                            {
                                float sdn = sd / 2304f;
                                moveDir += vm.normalized * (1f - sd) * evadeOthersMult;
                            }
                        }
                    }
                }
                targetChangeTimer += 0.5;
            }
        }
    }
}
