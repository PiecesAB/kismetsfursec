using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

public static class BulletRegister
{
    public static HashSet<BulletObject> allType1 = new HashSet<BulletObject>();
    public static BulletObject[] idList = new BulletObject[MAX_BULLETS]; // redundant second list for accessing via a unique ID
    private static int freeIdSpace = 0;

    public static HashSet<BulletObject> toDelete1 = new HashSet<BulletObject>();
    public static NativeArray<int> idsToDelete = new NativeArray<int>(MAX_BULLETS, Allocator.Persistent); // initializes all to -1 in the BulletControllerHelper.

    public const int MAX_BULLETS = 32768;

    public static List<Material> materials = new List<Material>();
    private static int freeMaterialSpace = 0;
    public static Dictionary<int, int> materialUsers = new Dictionary<int, int>(); // counts number of users

    public static List<Texture> textures = new List<Texture>();
    private static int freeTextureSpace = 0;
    public static Dictionary<int, int> textureUsers = new Dictionary<int, int>(); // counts number of users

    /*public static Dictionary<RenderGroup, MatrixArray> renderGroups = new Dictionary<RenderGroup, MatrixArray>(new RenderGroupEqualComp());

    public class MatrixArray // made to help with Graphics.DrawMeshInstanced
    {
        public List<List<Matrix4x4>> matrices;
        public int count;
        public const int maxSize = 1000;

        public int Add(Matrix4x4 m) // returns the index of what was added
        {
            if (matrices.Count == 0) { matrices.Add(new List<Matrix4x4>()); }
            if (matrices[matrices.Count - 1].Count >= maxSize) { matrices.Add(new List<Matrix4x4>()); }
            matrices[matrices.Count - 1].Add(m);

            ++count;

            return count - 1;
        }

        public void RemoveAt(int c)
        {
            if (c < 0 || c >= count) { return; }

            int i = 0;
            int offset = 0;
            for (int l = 0; l < matrices.Count; ++l)
            {
                if (offset + matrices[l].Count > c) { i = l; break; }
                offset += matrices[l].Count;
            }
            c -= offset;
            matrices[i].RemoveAt(c);

            if (matrices[i].Count == 0) { matrices.RemoveAt(i); }

            // merge small lists
            for (int l = 0; l < matrices.Count - 1; ++l)
            {
                int total = matrices[l].Count + matrices[l + 1].Count;
                if (total <= maxSize)
                {
                    List<Matrix4x4> temp = matrices[l]; //reference
                    matrices[l] = new List<Matrix4x4>();
                    matrices[l].AddRange(temp); matrices[l].AddRange(matrices[l + 1]);
                    matrices.RemoveAt(l + 1);
                    --l;
                }
            }
        }

        public Matrix4x4 this[int c]
        {
            get {
                if (c < 0 || c >= count) { throw new System.Exception("MatrixArray out of bounds"); }

                int i = 0;
                int offset = 0;
                for (int l = 0; l < matrices.Count; ++l)
                {
                    if (offset + matrices[l].Count > c) { i = l; break; }
                    offset += matrices[l].Count;
                }
                c -= offset;
                return matrices[i][c];
            }
            set {
                if (c < 0 || c >= count) { throw new System.Exception("MatrixArray out of bounds"); }

                int i = 0;
                int offset = 0;
                for (int l = 0; l < matrices.Count; ++l)
                {
                    if (offset + matrices[l].Count > c) { i = l; break; }
                    offset += matrices[l].Count;
                }
                c -= offset;
                matrices[i][c] = value;
            }
        }

        public MatrixArray()
        {
            matrices = new List<List<Matrix4x4>>() { new List<Matrix4x4>() };
            count = 0;
        }
    }

    public class RenderGroup
    {
        public Material material;
        public Texture image;
        public Color color;

        public RenderGroup(Material mat, Texture tex, Color col)
        {
            material = mat; image = tex; color = col;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RenderGroup);
        }

        public bool Equals(RenderGroup other)
        {
            return (other != null) && material == other.material && image == other.image && color == other.color;
        }

        public override int GetHashCode()
        {
            return material.GetHashCode() ^ image.GetHashCode() ^ color.GetHashCode();
        }
    }

    public class RenderGroupEqualComp : IEqualityComparer<RenderGroup>
    {
        public bool Equals(RenderGroup b1, RenderGroup b2)
        {
            if (b2 == null && b1 == null)
                return true;
            else if (b1 == null || b2 == null)
                return false;
            else if (b1.material == b2.material && b1.image == b2.image && b1.color == b2.color)
                return true;
            else
                return false;
        }

        public int GetHashCode(RenderGroup r)
        {
            return r.material.GetHashCode() ^ r.image.GetHashCode() ^ r.color.GetHashCode();
        }
    }*/

    private static void NextMaterialFreeSpace()
    {
        while (materials[freeMaterialSpace] != null)
        {
            ++freeMaterialSpace;
            if (freeMaterialSpace >= materials.Count)
            {
                break;
            }
        }
    }

    private static void NextTextureFreeSpace()
    {
        while (textures[freeTextureSpace] != null)
        {
            ++freeTextureSpace;
            if (freeTextureSpace >= textures.Count)
            {
                break;
            }
        }
    }

    public static bool IsRegistered(BulletObject b)
    {
        if (b == null) { return false; }
        return allType1.Contains(b);
    }

    public static void Register(ref BulletObject b, Material mat, Texture img)
    {
        if (b == null) { return; }
        if (allType1.Count >= MAX_BULLETS) { return; }

        allType1.Add(b);

        idList[freeIdSpace] = b;
        b.internalSelfId = freeIdSpace;
        while (idList[freeIdSpace] != null) { ++freeIdSpace; }

        int matIndex = materials.IndexOf(mat);
        if (matIndex == -1)
        {
            if (freeMaterialSpace >= materials.Count) { materials.Add(mat); b.materialInternalIdx = materials.Count - 1; ++freeMaterialSpace; }
            else { materials[freeMaterialSpace] = mat; b.materialInternalIdx = freeMaterialSpace; NextMaterialFreeSpace(); }
        }
        else
        {
            b.materialInternalIdx = matIndex;
        }
        if (!materialUsers.ContainsKey(b.materialInternalIdx)) { materialUsers[b.materialInternalIdx] = 0; }
        ++materialUsers[b.materialInternalIdx];

        int texIndex = textures.IndexOf(img);
        if (texIndex == -1)
        {
            if (freeTextureSpace >= textures.Count) { textures.Add(img); b.textureInternalIdx = textures.Count - 1; ++freeTextureSpace; }
            else { textures[freeTextureSpace] = img; b.textureInternalIdx = freeTextureSpace; NextTextureFreeSpace(); }
        }
        else
        {
            b.textureInternalIdx = texIndex;
        }
        if (!textureUsers.ContainsKey(b.textureInternalIdx)) { textureUsers[b.textureInternalIdx] = 0; }
        ++textureUsers[b.textureInternalIdx];

        b.renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
        /*if (!renderGroups.ContainsKey(b.renderGroup)) { renderGroups.Add(b.renderGroup, new MatrixArray()); }
        int ret = renderGroups[b.renderGroup].Add(b.renderMatrix);
        b.renderGroupIndex = ret;*/
    }

    /*public static void UpdateTransform(BulletObject b)
    {
        RenderGroup comp = b.renderGroup;
        if (!renderGroups.ContainsKey(comp)) { return; }
        renderGroups[comp][b.renderGroupIndex] = b.renderMatrix;
    }*/

    public class ClearReason { }
    public class SoftClear : ClearReason { }
    public class ClearFromAmbush : SoftClear { }
    public class ClearFromBossEndingTheBarrage : SoftClear { }

    public static void Clear(ClearReason cr = null)
    {
        foreach (BulletObject b in allType1)
        {
            if (cr is SoftClear && !b.destroyOnAmbushClear) { continue; } 
            MarkToDestroy(b);
        }
    }

    public static void ClearNonScrolling()
    {
        foreach (BulletObject b in allType1)
        {
            if (!b.destroyOnLeaveScreen && !b.destroyOnScreenScroll) { continue; }
            MarkToDestroy(b);
        }
    }

    public static void MarkToDestroy(BulletObject b, bool cancelGraphic = true)
    {
        if (b == null) { return; }
        b.timeWhenDestroyed = DoubleTime.ScaledTimeSinceLoad;
        if (b.materialInternalIdx >= 0 && materialUsers.ContainsKey(b.materialInternalIdx))
        {
            --materialUsers[b.materialInternalIdx];
            if (materialUsers[b.materialInternalIdx] == 0)
            {
                materialUsers.Remove(b.materialInternalIdx);
                materials[b.materialInternalIdx] = null; // this is not a deletion.
                freeMaterialSpace = Mathf.Min(freeMaterialSpace, b.materialInternalIdx);
            }
            b.materialInternalIdx = -1;
        }

        if (b.textureInternalIdx >= 0 && textureUsers.ContainsKey(b.textureInternalIdx))
        {
            --textureUsers[b.textureInternalIdx];
            if (textureUsers[b.textureInternalIdx] == 0)
            {
                textureUsers.Remove(b.textureInternalIdx);
                textures[b.textureInternalIdx] = null; // this is not a deletion.
                freeTextureSpace = Mathf.Min(freeTextureSpace, b.textureInternalIdx);
            }
            b.textureInternalIdx = -1;
        }

        if (b.internalSelfId >= 0)
        {
            idList[b.internalSelfId] = null;
            freeIdSpace = Mathf.Min(freeIdSpace, b.internalSelfId);
            b.internalSelfId = -1;
        }

        BinaryBullet.bullets.Remove(b);

        if (b.staticBulletParent)
        {
            b.staticBulletParent.AcknowledgeBulletWasDeleted(b.staticBulletIndex);
        }

        toDelete1.Add(b);

        if (cancelGraphic)
        {
            BulletCanceller.CreateImage(new Vector3(b.GetPosition().x, b.GetPosition().y, 1), 0.5f * (b.GetScale().x + b.GetScale().y), b.color);
        }
    }

    public static Material GetMaterial(ref BulletObject b)
    {
        if (b.materialInternalIdx == -1) { return null; }
        return materials[b.materialInternalIdx];
    }

    public static Material GetMaterial(BulletObject b)
    {
        if (b.materialInternalIdx == -1) { return null; }
        return materials[b.materialInternalIdx];
    }

    public static Texture GetImage(ref BulletObject b)
    {
        if (b.textureInternalIdx == -1) { return null; }
        return textures[b.textureInternalIdx];
    }

    public static Texture GetImage(BulletObject b)
    {
        if (b.textureInternalIdx == -1) { return null; }
        return textures[b.textureInternalIdx];
    }

    public static void Reregister(ref BulletObject b)
    {
        Material m = GetMaterial(ref b); Texture t = GetImage(ref b);
        MarkToDestroy(b, false);
        toDelete1.Remove(b);
        allType1.Remove(b);
        Register(ref b, m, t);
    }

    public static void Reregister(ref BulletObject b, Material m, Texture t)
    {
        MarkToDestroy(b, false);
        toDelete1.Remove(b);
        allType1.Remove(b);
        Register(ref b, m, t);
    }
}
