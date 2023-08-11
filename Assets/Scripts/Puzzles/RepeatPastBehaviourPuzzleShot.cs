using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatPastBehaviourPuzzleShot : MonoBehaviour, IBulletMakerOnShot
{
    public RepeatPastBehaviourPuzzle target;
    public BulletHellMakerFunctions shooter;

    private void Start()
    {
        shooter = GetComponent<BulletHellMakerFunctions>();
    }

    public void BeforeShot()
    {
        if (!shooter) { return; }

        if (target.recording) {
            shooter.DisableFireRightBeforeShot();
            return;
        }

        if (target.currFrame >= target.lastPlaybackFrame)
        {
            transform.eulerAngles = new Vector3(0, 0, Fakerand.Single()*360f);
            return;
        }

        Transform targTrans = target.transform;

        // Perform quadrature to find where to shoot
        int fmax = target.lastPlaybackFrame;
        int currFrameOfImpact = Mathf.Min(target.currFrame, fmax);
        int lastFrameOfImpact = -1;
        int attempts = 32;
        List<int> attemptList = new List<int>(32);
        while (attempts > 0 && lastFrameOfImpact != currFrameOfImpact)
        {
            Vector2 currPos = target.positions[currFrameOfImpact];
            float currDist = Vector2.Distance(currPos, transform.position);
            int framesToReachCurr = Mathf.RoundToInt(currDist * 60f / shooter.bulletData.speed);
            lastFrameOfImpact = currFrameOfImpact;
            currFrameOfImpact = Mathf.Min(target.currFrame + framesToReachCurr, fmax);
            int sameAttemptIndex = attemptList.IndexOf(currFrameOfImpact);
            if (sameAttemptIndex != -1)
            {
                int c = 1;
                float avg = 0f;
                for (int i = sameAttemptIndex; i < attemptList.Count; ++i)
                {
                    float rat = 1f / c;
                    avg = rat * attemptList[i] + (1f - rat) * avg;
                    ++c;
                }
                currFrameOfImpact = Mathf.RoundToInt(avg);
            }
            attemptList.Add(currFrameOfImpact);
            --attempts;
        }
        if (attempts == 0)
        {
            shooter.DisableFireRightBeforeShot();
            return;
        }

        Vector2 posToShoot = target.positions[currFrameOfImpact];
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(posToShoot.y - transform.position.y, posToShoot.x - transform.position.x) * Mathf.Rad2Deg);
    }

    public void OnShot()
    {
        
    }
}
