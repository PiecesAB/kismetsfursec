using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DrawStaticBullets))]
public class DrawStaticBulletsEditor : Editor
{
    private bool editing = false;
    private float spacing = 16f;
    private Vector2 lastPlaced = new Vector2(-1e6f, -1e6f);
    public GameObject holder;

    private void OnSceneGUI()
    {
        if (editing)
        {
            if (Event.current.type == EventType.MouseDrag)
            {
                Vector2 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                if ((lastPlaced - mousePos).magnitude >= spacing)
                {
                    lastPlaced = mousePos;
                    Transform bulletSampleTransform = ((DrawStaticBullets)target).transform.GetChild(0);
                    if (bulletSampleTransform == null) { Debug.LogError("No child bullet sample"); return; }
                    GameObject bulletSample = bulletSampleTransform.gameObject;
                    GameObject newBullet = Instantiate(bulletSample, bulletSampleTransform.parent);
                    newBullet.name = "BulletPlace";
                    newBullet.transform.position = new Vector3(mousePos.x, mousePos.y, bulletSampleTransform.position.z);
                }
                Event.current.Use();
                if (holder) { EditorUtility.SetDirty(holder); }
            }
        }

    }

    public override void OnInspectorGUI()
    {
        if (editing)
        {
            GUILayout.Label("Drag with middle or right mouse held down.");
            spacing = EditorGUILayout.FloatField("Spacing: ", spacing);
            if (GUILayout.Button("Stop draw"))
            {
                editing = false;
            }
        }
        else
        {
            if (GUILayout.Button("Draw"))
            {
                editing = true;
                lastPlaced = new Vector2(-1e6f, -1e6f);
            }
        }
    }
}
