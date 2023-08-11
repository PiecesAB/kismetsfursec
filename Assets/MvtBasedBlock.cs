using UnityEngine;
using System.Collections.Generic;

public class MvtBasedBlock : MonoBehaviour {

    public enum BehaviorType
    {
        EnabledOnMove,
        EnabledOnStatic,
        ToggledEveryJump,
        ToggleOnceOffScreen,
        EnabledOnAirborne
    }

    public BehaviorType behaviorType;
    public bool startsEnabled;
    public Sprite EnabledSpr;
    public Sprite DisabledSpr;
    public ParticleSystem changeParticles;

    public static AudioSource mvtBlockTick;

    private bool prvEnabled;
    private bool prvWasVisible;

    private Renderer rend;

    public static HashSet<MvtBasedBlock> all = new HashSet<MvtBasedBlock>();

    private int playerTouches = 0;
    private int airborneDisableGrace = 8;
    private int initAirborneDisableGrace = 8;

    private void TryToTick()
    {
        if (mvtBlockTick == null)
        {
            if (LevelInfoContainer.main)
            {
                foreach (var a in LevelInfoContainer.main.transform.GetComponentsInChildren<AudioSource>())
                {
                    if (a.gameObject.name == "mvtBlockTick")
                    {
                        mvtBlockTick = a;
                        break;
                    }
                }
            }
        }

        if (mvtBlockTick && (rend?.isVisible ?? false))
        {
            mvtBlockTick.Stop();
            mvtBlockTick.Play();
        }
    }

    void Enable()
    {
        if (!prvEnabled)
        {
            if (behaviorType == BehaviorType.EnabledOnAirborne)
            {
                airborneDisableGrace = initAirborneDisableGrace;
            }
            prvEnabled = true;
            GetComponent<SpriteRenderer>().sprite = EnabledSpr;
            GetComponent<Collider2D>().enabled = true;
            TryToTick();
        }
    }
    void Disable()
    {
        if (prvEnabled)
        {
            if (behaviorType == BehaviorType.EnabledOnAirborne)
            {
                --airborneDisableGrace;
                if (airborneDisableGrace > 0) { return; }
            }
            prvEnabled = false;
            GetComponent<SpriteRenderer>().sprite = DisabledSpr;
            GetComponent<Collider2D>().enabled = false;
            TryToTick();
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 20)
        {
            ++playerTouches;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.layer == 20)
        {
            --playerTouches;
        }
    }

    public void Update()
    {
        if (behaviorType == BehaviorType.ToggleOnceOffScreen)
        {
            bool vis = rend.isVisible;
            if (vis != prvWasVisible)
            {
                if (prvWasVisible)
                {
                    if (prvEnabled)
                    {
                        Disable();
                    }
                    else
                    {
                        Enable();
                    }
                    if (changeParticles)
                    {
                        changeParticles.Stop();
                        changeParticles.Play();
                    }
                }
                prvWasVisible = vis;
            }
        }

        if (behaviorType == BehaviorType.EnabledOnAirborne && rend.isVisible)
        {
            Encontrolmentation e = LevelInfoContainer.GetActiveControl();
            if (!e) { return; }
            BasicMove bm = e.GetComponent<BasicMove>();
            if (bm.grounded != 0 && playerTouches == 0) { Disable(); }
            else { Enable(); }
        }
    }

    public void TRIGGERED(byte info)
    {
        if (behaviorType == BehaviorType.EnabledOnMove)
        {
            if ((info & 3) != 0)
            {

                    Enable();

            }
            else
            {
                    Disable();
            }
        }
        if (behaviorType == BehaviorType.EnabledOnStatic)
        {
            if ((info & 3) == 0)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        if (behaviorType == BehaviorType.ToggledEveryJump)
        {
            if ((info & 4) == 4)
            {
                if (!prvEnabled)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

    }

    private void Awake()
    {
        all.Add(this);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    void Start () {
        rend = GetComponent<Renderer>();
        if (behaviorType == BehaviorType.ToggledEveryJump 
            || behaviorType == BehaviorType.ToggleOnceOffScreen)
        {
            prvWasVisible = false;
            if (startsEnabled)
            {
                prvEnabled = false;
                Enable();
            }
            else
            {
                prvEnabled = true;
                Disable();
            }
        }
        else
        {
            prvEnabled = true;
            TRIGGERED(0);
        }
	}
	
}
