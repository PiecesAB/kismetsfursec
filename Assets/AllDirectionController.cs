using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllDirectionController : MonoBehaviour, IExaminableAction
{
    //e is null: do default movement. e exists: take control
    public Encontrolmentation e;
    public GameObject controlTarget;
    public Transform joystick;
    public Transform button;
    public Renderer visibilityCheck;
    public Vector2 direction;
    public bool trigger;

    private float store1;

    private const float mvtAmt = 25f;
    private static Vector3 defaultPos = new Vector3(-90f, 0f, 0f);

    void Start()
    {
        joystick.localEulerAngles = defaultPos;
    }

    public void OnExamine(Encontrolmentation plr)
    {
        if (controlTarget == null) { return; }

        BasicMove bm = plr.GetComponent<BasicMove>();
        FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
        if (e)
        {
            e = null;
            if (bm) { bm.disabledAllControlMvt = false; bm.fakePhysicsVel.x = 0f; }
            if (ftp) { ftp.target = bm.transform; }
            ChangeState(false);
        }
        else
        {
            e = plr;
            if (bm) { bm.disabledAllControlMvt = true; bm.fakePhysicsVel.x = 0f; }
            if (ftp) { ftp.target = controlTarget.transform; }
            ChangeState(true);
        }
    }

    public void ChangeState(bool newState)
    {
        if (e && !controlTarget)
        {
            e = null;
            return;
        }

        if (controlTarget)
        {
            crawler1 c1 = controlTarget.GetComponent<crawler1>();
            if (c1)
            {
                if (newState) { c1.beingControlled = this; c1.speed = 1f; }
                else { c1.beingControlled = null; }
            }
        }
    }
    
    void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (!e)
        {
            return;
        }

        if (!controlTarget)
        {
            e = null;
            return;
        }

        if (visibilityCheck.isVisible || e)
        {
            if (e)
            {
                button.localScale = Vector3.MoveTowards(button.localScale,Vector3.one,0.4f);
                direction = Vector2.zero;
                if ((e.currentState & 1UL) == 1UL) { direction += Vector2.left * e.horizontalPressure; }
                if ((e.currentState & 2UL) == 2UL) { direction += Vector2.right * e.horizontalPressure; }
                if ((e.currentState & 4UL) == 4UL) { direction += Vector2.up * e.verticalPressure; }
                if ((e.currentState & 8UL) == 8UL) { direction += Vector2.down * e.verticalPressure; }
                Vector3 displace = new Vector3(-direction.y, -direction.x);
                trigger = e.ButtonDown(16UL, 16UL);
                if (trigger) { button.localScale = 3f * Vector3.one; }
                displace *= mvtAmt;
                joystick.localEulerAngles = defaultPos + displace;
            }
            else
            {

                float t = (float)(DoubleTime.ScaledTimeSinceLoad % 1.0) * 6.2831853f;
                Vector3 displace = mvtAmt * (new Vector3(Mathf.Cos(t), Mathf.Sin(t)));
                joystick.localEulerAngles = defaultPos + displace;
                direction = Vector2.zero;
                trigger = false;
            }
        }

        
    }
}
