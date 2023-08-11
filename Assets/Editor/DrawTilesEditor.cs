using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DrawTiles))]
public class DrawTilesEditor : Editor
{
    private bool drawing = false;
    private bool erasing = false;
    private bool variantEditorOpen = false;
    private Transform tile;
    private Vector2 tileSize;
    private Vector2 customTileSize;

    private class TilePositionEqual : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) < 1 && Mathf.Abs(a.y - b.y) < 1;
        }

        public int GetHashCode(Vector2 obj)
        {
            return 19701 * (int)obj.x + (int)obj.y;
        }
    }

    private Dictionary<Vector2, GameObject> posToTile = new Dictionary<Vector2, GameObject>(new TilePositionEqual());
    private Transform thisT;

    private Vector2 SnapPosition(Vector2 p)
    {
        DrawTiles cTarget = (DrawTiles)target;
        if (cTarget.tileSizeOverride.magnitude > 0)
        {
            tileSize = cTarget.tileSizeOverride;
        }
        Vector2 rPos = p - (Vector2) tile.position;
        return new Vector2(Mathf.Round(rPos.x / tileSize.x) * tileSize.x, Mathf.Round(rPos.y / tileSize.y) * tileSize.y) + (Vector2) tile.position;
    }

    private Vector2 TileCoordinatePosition(Vector2 p)
    {
        DrawTiles cTarget = (DrawTiles)target;
        if (cTarget.tileSizeOverride.magnitude > 0)
        {
            tileSize = cTarget.tileSizeOverride;
        }
        Vector2 rPos = p - (Vector2)tile.position;
        return new Vector2(Mathf.Round(rPos.x / tileSize.x), Mathf.Round(rPos.y / tileSize.y));
    }

    private void OnSceneGUI()
    {
        if (drawing || erasing)
        {
            if (Event.current.type == EventType.MouseDrag)
            {
                Vector2 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                Vector2 rPos = SnapPosition(mousePos);

                Event.current.Use();

                if (posToTile.ContainsKey(rPos))
                {
                    if (erasing)
                    {
                        DestroyImmediate(posToTile[rPos]);
                        if (tile == null) { SetTileInfo(); }
                        posToTile.Remove(rPos);
                    }
                }
                else
                {
                    if (drawing)
                    {
                        GameObject oldTile = tile.gameObject;
                        Object oldSelect = Selection.activeObject;
                        Selection.activeObject = oldTile;
                        Unsupported.CopyGameObjectsToPasteboard();
                        Unsupported.PasteGameObjectsFromPasteboard();
                        GameObject newTile = (GameObject)Selection.activeObject;
                        Selection.activeObject = oldSelect;
                        newTile.transform.position = new Vector3(rPos.x, rPos.y, tile.position.z);
                        newTile.transform.rotation = tile.rotation;
                        newTile.transform.SetParent(tile.parent);
                        posToTile[rPos] = newTile;
                    }
                }
                
            }
        }
    }

    private void SetTileInfo()
    {
        DrawTiles cTarget = (DrawTiles)target;
        thisT = ((MonoBehaviour)target).transform;
        if (thisT.childCount == 0) { drawing = false; Debug.LogError("No tile inside this! Add a sample tile."); }
        tile = thisT.GetChild(0);
        SpriteRenderer tileSR = tile.GetComponent<SpriteRenderer>();
        if (tileSR)
        {
            tileSize = (tileSR.drawMode != SpriteDrawMode.Simple) ? tileSR.size : (Vector2)tileSR.sprite.bounds.size;
        }
        else
        {
            tileSize = new Vector2(16, 16);
        }
        if (cTarget.tileSizeOverride.magnitude > 0)
        {
            tileSize = cTarget.tileSizeOverride;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawTiles cTarget = (DrawTiles)target;

        if (variantEditorOpen)
        {
            GUILayout.Label("Draw tiles first, then apply variants.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variants"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variantPatternType"), true);
            switch (cTarget.variantPatternType)
            {
                case DrawTiles.VariantPatternType.Image:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("image"), true);
                    break;
                case DrawTiles.VariantPatternType.Perlin:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("perlinParameters"), true);
                    break;
                case DrawTiles.VariantPatternType.Random:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("randomChance"), true);
                    break;
                case DrawTiles.VariantPatternType.Gradient:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gradient"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gradientStart"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gradientEnd"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gradientRepeat"), true);
                    break;
            }
            if (GUILayout.Button("Apply Variants"))
            {
                if (cTarget.variants.Length > 0)
                {
                    SetTileInfo();
                    List<Transform> original = new List<Transform>();
                    foreach (Transform t in thisT)
                    {
                        if (t == thisT) { continue; }
                        original.Add(t);
                    }
                    foreach (Transform t in original)
                    {
                        if (t == thisT) { continue; }
                        Vector2 coord = TileCoordinatePosition(t.position);
                        GameObject sample = cTarget.GetVariantAtPosition(coord);
                        if (sample == null) { continue; }
                        Object oldSelect = Selection.activeObject;
                        Selection.activeObject = sample;
                        Unsupported.CopyGameObjectsToPasteboard();
                        Unsupported.PasteGameObjectsFromPasteboard();
                        GameObject replacement = (GameObject)Selection.activeObject;
                        replacement.transform.position = t.position;
                        replacement.transform.rotation = t.rotation;
                        replacement.transform.SetParent(t.parent);
                        Selection.activeObject = oldSelect;
                        if (((DrawTiles)target).variantPatternType == DrawTiles.VariantPatternType.Gradient && replacement.GetComponent<SpriteRenderer>())
                        {
                            var gradient = ((DrawTiles)target).gradient;
                            var gradientStart = ((DrawTiles)target).gradientStart;
                            var gradientEnd = ((DrawTiles)target).gradientEnd;
                            var gradientRepeat = ((DrawTiles)target).gradientRepeat;
                            if (gradientStart == gradientEnd) { Debug.LogError("Gradient start and end point are same"); }
                            else {
                                Vector2 dif = gradientEnd - gradientStart;
                                float gradPos = Vector2.Dot(dif.normalized, (Vector2)t.position - gradientStart) / dif.magnitude;
                                switch (gradientRepeat)
                                {
                                    case DrawTiles.GradientRepeatType.Clamp:
                                        gradPos = Mathf.Clamp01(gradPos);
                                        break;
                                    case DrawTiles.GradientRepeatType.Repeat:
                                        gradPos = Mathf.Repeat(gradPos, 1f);
                                        break;
                                    case DrawTiles.GradientRepeatType.Reflect:
                                        gradPos = Mathf.PingPong(gradPos, 1f);
                                        break;
                                }
                                replacement.GetComponent<SpriteRenderer>().color = gradient.Evaluate(gradPos);
                            }
                        }
                        if (t == tile) { tile = replacement.transform; }
                        DestroyImmediate(t.gameObject);
                    }
                }
                else { Debug.LogError("There are no variants to apply."); }
            }
            if (GUILayout.Button("Close Variant Menu"))
            {
                variantEditorOpen = false;
            }
        }
        else
        {
            if (GUILayout.Button("Open Variant Menu"))
            {
                variantEditorOpen = true;
            }
        }

        GUILayout.Space(10);

        if (drawing)
        {
            GUILayout.Label("Drag with middle or right mouse held down.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSizeOverride"), true);
            if (GUILayout.Button("Stop draw"))
            {
                drawing = erasing = false;
            }
        }
        else
        {
            if (GUILayout.Button("Draw"))
            {
                drawing = true;
                erasing = false;
                SetTileInfo();
                posToTile.Clear();
                foreach (Transform t in thisT)
                {
                    if (t == thisT) { continue; }
                    posToTile[t.position] = t.gameObject;
                }
            }
        }

        if (erasing)
        {
            GUILayout.Label("Drag with middle or right mouse held down.");
            if (GUILayout.Button("Stop Erase"))
            {
                drawing = erasing = false;
            }
        }
        else
        {
            if (GUILayout.Button("Erase"))
            {
                drawing = false;
                erasing = true;
                SetTileInfo();
                posToTile.Clear();
                foreach (Transform t in thisT)
                {
                    if (t == thisT) { continue; }
                    posToTile[t.position] = t.gameObject;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
