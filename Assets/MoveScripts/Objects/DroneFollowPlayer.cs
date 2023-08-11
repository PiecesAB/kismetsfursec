using UnityEngine;
using System.Collections;
using System;

public class DroneFollowPlayer : GenericBlowMeUp {

    public enum MvtType
    {
        DirectTowards, Timer
    }

    public MvtType mvtType = MvtType.DirectTowards;
    public EasingOfAccess.EasingType timerEasing = EasingOfAccess.EasingType.ElasticOut;

    public bool realTime = false;
    public double timeLeft = 0.0;

    public float speed;
    public float maxSpeed;

    public GameObject heldItem;

    public float randomRadiusOffset = 0f;
    private Vector2 randomOffset = Vector2.zero;

    private Vector3 direction;
    private float mag;

    private Rigidbody2D r2;

    private void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        if (heldItem)
        {
            heldItem.transform.SetParent(transform);
            heldItem.transform.localPosition = new Vector3(0, -16);
        }
        if (randomRadiusOffset > 0)
        {
            randomOffset = Fakerand.UnitCircle() * randomRadiusOffset;
        }
        MoveStep();
    }

    private void MoveStep()
    {
        if (!LevelInfoContainer.GetActiveControl()) { return; }
        Vector3 plrPos = LevelInfoContainer.GetActiveControl().transform.position + (Vector3)randomOffset;
        switch (mvtType) {
            case MvtType.DirectTowards:
                direction = plrPos - transform.position;
                mag = direction.magnitude;
                if (mag > maxSpeed)
                {
                    mag = maxSpeed;
                }
                direction = Vector3.Normalize(direction);
                if (Vector2.MoveTowards(transform.position, plrPos, (mag / 5) * speed * Time.deltaTime) == (Vector2)plrPos)
                {
                    transform.position = plrPos;
                    r2.velocity = Vector2.zero;
                }
                else
                {
                    r2.velocity = (mag / 5) * speed * direction;
                }
                break;
            case MvtType.Timer:
                timeLeft -= (realTime ? 1f : Time.timeScale) * 0.0166666666666666;
                timeLeft = Math.Max(0f, timeLeft);
                float baseHeight = Mathf.Floor((float)timeLeft) * speed;
                float secondHeight = EasingOfAccess.Evaluate(timerEasing, (float)(timeLeft % 1.0)) * speed;
                if (timeLeft >= 1f)
                {
                    baseHeight += 48f;
                }
                else
                {
                    secondHeight *= 48f / speed;
                }
                Vector2 furtherOffset = heldItem ? heldItem.transform.position - transform.position : Vector3.zero;
                transform.position = plrPos + new Vector3(0, baseHeight + secondHeight, 0) + (Vector3)furtherOffset;
                break;
        }
    }

    private void Update () {
        if (Time.timeScale == 0) { return; }
        MoveStep();
	}
}
