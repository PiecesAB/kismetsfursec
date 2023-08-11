using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PrimBezierRender : MonoBehaviour
{
    //private Material mat;
    //public Material rend;
    //Texture2D tex;
    // Will be called from camera after regular rendering is done.

    public GameObject pivotSample;
    public LineRenderer line;
    public Bezier bezier = new Bezier();

    public bool checkMeToRender;

    public bool autoRenderInEditor = false;

    public bool updateCenter = false;

    public bool useInitialLocalSpace = false;

    public bool invisibleInPlay = false;

    private void Awake()
    {
        //tex = new Texture2D(640, 480);
        //GetComponent<Renderer>().material.mainTexture = tex;
        if (useInitialLocalSpace)
        {
            bezier.offset = transform.TransformPoint(Vector3.zero);
        }
    }

    public void Update()
    {
        if (checkMeToRender || (autoRenderInEditor && !Application.isPlaying))
        {
            checkMeToRender = false;
            bezier.offset = transform.position;
            bezier.DrawUsingStuff(ref pivotSample, ref line);
        }

        if (updateCenter)
        {
            bezier.offset = transform.position;
        }

        if (Application.isPlaying && line) { line.enabled = !invisibleInPlay; };
    }

    public void OnPostRender()
    {
        //print("bonk");
        /*  if (!mat)
          {
              // Unity has a built-in shader that is useful for drawing
              // simple colored things. In this case, we just want to use
              // a blend mode that inverts destination colors.
              var shader = Shader.Find("Hidden/Internal-Colored");
              mat = new Material(shader);
              mat.hideFlags = HideFlags.HideAndDontSave;
              // Set blend mode to invert destination colors.
              mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
              mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
              // Turn off backface culling, depth writes, depth test.
              mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
              mat.SetInt("_ZWrite", 0);
              mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
          }
          GL.PushMatrix();
          GL.LoadOrtho();

          // activate the first shader pass (in this case we know it is the only pass)
          mat.SetPass(0);
          // draw a quad over whole screen
          GL.Begin(GL.QUADS);
          GL.Vertex3(0.1f, 0.1f, 0);
          GL.Vertex3(0.9f, 0.1f, 0);
          GL.Vertex3(0.9f, 0.9f, 0);
          GL.Vertex3(0.1f, 0.9f, 0);
          GL.End();

          GL.PopMatrix();*/
       /* RenderTexture.active = null;
        tex.ReadPixels(new Rect(0, 0, 640, 480), 0, 0);
        for (int i = 0; i < 640; i++)
            for (int j = 0; j < 480; j++)
            {
                tex.SetPixel(i+100, j+100, Color.red);
            }
        tex.Apply();*/
    }


}
