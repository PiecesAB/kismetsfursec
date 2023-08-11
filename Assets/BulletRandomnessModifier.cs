using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletRandomnessModifier : MonoBehaviour, IBulletMakerOnShot
{
    public float waitTimeVariation = 0f;
    private float originalWaitTime;
    public float speedVariation = 0f;
    private float originalSpeed;
    public float torqueVariation = 0f;
    private float originalTorque;

    private BulletHellMakerFunctions maker;

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        originalWaitTime = maker.waitTime;
        originalSpeed = maker.bulletData.speed;
        originalTorque = maker.bulletData.torque;
    }

    private void OnEnable()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        originalWaitTime = maker.waitTime;
        originalSpeed = maker.bulletData.speed;
        originalTorque = maker.bulletData.torque;
    }

    public void PreResetOnSuperRepeatMod()
    {
        maker.waitTime = originalWaitTime;
        maker.bulletData.speed = originalSpeed;
        maker.bulletData.torque = originalTorque;
    }

    public void ResetOnSuperRepeatMod()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        originalWaitTime = maker.waitTime;
        originalSpeed = maker.bulletData.speed;
        originalTorque = maker.bulletData.torque;
    }

    public void BeforeShot()
    {
        if (!maker) { maker = GetComponent<BulletHellMakerFunctions>(); }
        maker.waitTime = Fakerand.Single(originalWaitTime - waitTimeVariation, originalWaitTime + waitTimeVariation);
        maker.bulletData.speed = Fakerand.Single(originalSpeed - speedVariation, originalSpeed + speedVariation);
        maker.bulletData.torque = Fakerand.Single(originalTorque - torqueVariation, originalTorque + torqueVariation);
    }

    public void OnShot() { }
}