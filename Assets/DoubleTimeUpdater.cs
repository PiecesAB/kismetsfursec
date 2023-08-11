using UnityEngine;

[ExecuteInEditMode]
public class DoubleTimeUpdater : MBSingleton<DoubleTimeUpdater>
{
    protected override void ChildAwake()
    {
        Application.targetFrameRate = 60;
        Settings.UpdateAll();
    }

    private void OnLevelWasLoaded(int level)
    {
        DoubleTime.ScaledTimeSinceLoad = DoubleTime.UnscaledTimeSinceLoad = 0f;
    }

    private void Update()
    {
        DoubleTime.AddToTime(0.0166666666666666666666);
        if (Time.timeScale > 0) {
            Physics2D.Simulate(0.016666666666f * Time.timeScale);
        }
        Prim3DRotate.SharedUpdate();
        BoostArrow.SharedUpdate();
        Shader.SetGlobalFloat("_DTScaledTimeSinceLoad", (float)(DoubleTime.ScaledTimeSinceLoad % 10000.0));
    }
}
