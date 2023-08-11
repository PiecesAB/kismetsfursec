using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranquilizerSpeedMod : MonoBehaviour
{
    public float speedChange = 0f;
    public float jumpHeightChange = 0f;
    public float punchPowerMultChange = 0f;
    public float maxFallSpeedMultChange = 0f;
    public bool jumpDisabled = false;
    public bool removeJumpWhenNotGrounded = false;
    public bool refreshDoubleJumpOnRecover = false; // once this is removed

    private BasicMove bm;

    public void RemoveTranquilizer()
    {
        bm.moveSpeed -= speedChange;
        bm.jumpHeight -= jumpHeightChange;
        bm.punchPowerMultiplier -= punchPowerMultChange;
        bm.maxFallSpeedTranqMult -= maxFallSpeedMultChange;
        bm.youCanJump = true;
        if (refreshDoubleJumpOnRecover)
        {
            bm.doubleJump = true;
        }
        Destroy(this);
    }

    private void Start()
    {
        bm = GetComponent<BasicMove>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        // lanugo
        if (removeJumpWhenNotGrounded)
        {
            if (bm.grounded == 0)
            {
                bm.youCanJump = false;
                bm.doubleJump = false;
            }
            else
            {
                bm.youCanJump = true;
            }
        }
    }
}
