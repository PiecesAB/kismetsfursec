using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Reflection;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteShapeController))]
public class ModulatingGround : MonoBehaviour {

    public enum Waveform
    {
        Square, Sine, Triangle, Sawtooth, ConstantOne
    }

    [System.Serializable]
    public struct Wave
    {
        public Waveform waveform;
        public float frequency;
        public float amplitude;
        public float phaseOffset;
        public float phaseOverTime;
    }

    public SpriteShapeController ssc;
    public List<Wave> waves = new List<Wave>();
    public float width;
    public AnimationCurve baseHeight;
    public float baseHeightMultiplier;
    public AnimationCurve unitHeight;
    public float unitHeightMultiplier;
    public Vector2 heightRange;
    public int subdivisions;
    private Dictionary<string, MethodInfo> waveMethods = new Dictionary<string, MethodInfo>();

    void AddMethods()
    {
        foreach (MethodInfo i in Type.GetType("WaveFunctions").GetMethods())
        {
            waveMethods.Add(i.Name, i);
        }
    }

	void Start () {
        AddMethods();
	}

    float Calc(float x)
    {
        float y = 0f;
        for (int i = 0; i < waves.Count; i++)
        {
            Wave w = waves[i];
            object[] tmp = new object[1] { (x * w.frequency) + w.phaseOffset + (w.phaseOverTime * DoubleTime.ScaledTimeSinceLoad) };
            float r = (float)waveMethods[w.waveform.ToString()].Invoke(null, tmp);
            y += r * w.amplitude;
        }
        return y;
    }
	
	void Update () {
        if (waveMethods.Count == 0) //editor
        {
            AddMethods();
        }
        if (GetComponent<Renderer>().isVisible)
        {
            Spline s = ssc.spline;
            float w_2 = width * 0.5f;
            s.Clear();
            s.InsertPointAt(0, new Vector3(-w_2, 0));
            s.SetTangentMode(0, ShapeTangentMode.Continuous);
            s.InsertPointAt(0, new Vector3(w_2, 0));
            s.SetTangentMode(0, ShapeTangentMode.Continuous);
            for (int p = 0; p <= subdivisions; p++)
            {
                float prog = (1f - (p * 2f / subdivisions));
                float prog_2 = (1f - ((float)p / subdivisions));
                float bh = (baseHeight.Evaluate(prog_2) * baseHeightMultiplier);
                float uh = (unitHeight.Evaluate(prog_2) * unitHeightMultiplier);
                s.InsertPointAt(0, new Vector3(prog * w_2, Mathf.Clamp(bh + (uh * Calc(prog_2)), heightRange.x, heightRange.y)));
                s.SetTangentMode(0, ShapeTangentMode.Continuous);
            }
            
            ssc.BakeCollider();
        }
    }
}
