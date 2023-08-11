using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AmorphousColliderMeld
{
    public class BlockPosComparer : IComparer<AmorphousGroundTileNormal>
    {
        public int Compare(AmorphousGroundTileNormal a, AmorphousGroundTileNormal b)
        {
            Vector2 aPos = a.transform.position;
            Vector2 bPos = b.transform.position;
            if (aPos.y != bPos.y) { return Mathf.RoundToInt(bPos.y - aPos.y); }
            if (aPos.x != bPos.x) { return Mathf.RoundToInt(aPos.x - bPos.x); }
            return (int)((bPos.y + aPos.x) - (aPos.y + bPos.x));
        }
    }

    public static void CreateMeld()
    {
        if (!Application.isPlaying || blocksToMeld.Count == 0) { return; }

        GameObject meld = new GameObject("AmorphousTile Static Meld"); //destroyed when level is reloaded: different per each level
        meld.transform.SetParent(blocksToMeld.Min.transform.parent);
        meld.layer = 8; //ground layer

        // this is O(n log n) with the number of blocks, due to the remove operation.
        while (blocksToMeld.Count > 0)
        {
            AmorphousGroundTileNormal start = blocksToMeld.Min;
            start.GetComponent<Collider2D>().enabled = false;
            Vector2 startPos = start.transform.position;
            blocksToMeld.Remove(start);
            int width = 1; int height = 1;
            bool checkAgain = true; bool xDone = false; bool yDone = false;
            while (checkAgain)
            {
                checkAgain = false;

                // try to extend the rectangle right
                if (!xDone)
                {
                    bool xExtend = true;
                    AmorphousGroundTileNormal[] xRemove = new AmorphousGroundTileNormal[height];
                    for (int y = 0; y < height; ++y)
                    {
                        AmorphousGroundTileNormal got = AmorphousGroundTileNormal.allBlocks.Fetch(startPos + new Vector2(16 * width, -16 * y));
                        if (got != null && !blocksToMeld.Contains(got)) { got = null; }
                        if (got == null || got.databaseObject != start.databaseObject) { xExtend = false; xDone = true; break; } // no more going right
                        else { xRemove[y] = got; }
                    }
                    if (xExtend)
                    {
                        checkAgain = true;
                        ++width;
                        for (int y = 0; y < height; ++y) {
                            blocksToMeld.Remove(xRemove[y]);
                            xRemove[y].GetComponent<Collider2D>().enabled = false;
                        }
                    }
                }

                // try to extend the rectangle down
                if (!yDone)
                {
                    bool yExtend = true;
                    AmorphousGroundTileNormal[] yRemove = new AmorphousGroundTileNormal[width];
                    for (int x = 0; x < width; ++x)
                    {
                        AmorphousGroundTileNormal got = AmorphousGroundTileNormal.allBlocks.Fetch(startPos + new Vector2(16 * x, -16 * height));
                        if (got != null && !blocksToMeld.Contains(got)) { got = null; }
                        if (got == null || got.databaseObject != start.databaseObject) { yExtend = false; yDone = true; break; } // no more going down
                        else { yRemove[x] = got; }
                    }
                    if (yExtend)
                    {
                        checkAgain = true;
                        ++height;
                        for (int x = 0; x < width; ++x) {
                            blocksToMeld.Remove(yRemove[x]);
                            yRemove[x].GetComponent<Collider2D>().enabled = false;
                        }
                    }
                }
                
            }

            //create the combined collider
            if (width > 1 || height > 1)
            {
                // we have to use separate blocks or the players' ceiling hits will screw up.
                GameObject subMeld = new GameObject("SubMeld");
                subMeld.transform.SetParent(meld.transform);
                subMeld.transform.position = start.transform.position;
                subMeld.layer = 8; //ground layer
                Rigidbody2D r2 = subMeld.AddComponent<Rigidbody2D>();
                r2.isKinematic = true;
                r2.sleepMode = RigidbodySleepMode2D.StartAsleep;
                BoxCollider2D col = subMeld.AddComponent<BoxCollider2D>();
                col.size = new Vector2(16 * width, 16 * height);
                col.offset = new Vector2(8 * (width - 1), -8 * (height - 1));
                if (start.GetComponent<SpriteRenderer>().sprite.name.StartsWith(Utilities.imperviousBlockName))
                {
                    primExtraTags pet = subMeld.AddComponent<primExtraTags>();
                    pet.tags = new List<string>() { "ImperviousBlock" };
                }
            }
            else
            {
                start.GetComponent<Collider2D>().enabled = true;
            }

        }


    }

    public static SortedSet<AmorphousGroundTileNormal> blocksToMeld = new SortedSet<AmorphousGroundTileNormal>(new BlockPosComparer());


}
