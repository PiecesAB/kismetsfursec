using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTiles : MonoBehaviour
{
    public GameObject[] variants;

    // Perlin: interpolate tiles based on scaled perlin noise
    // Image: use a grayscale image to interpolate tiles
    // Random: by a random chance, replace some tiles with a variant
    public enum VariantPatternType
    {
        Image, Perlin, Random, Gradient
    }

    public VariantPatternType variantPatternType;
    public Vector2 perlinParameters;
    public Texture2D image;
    public float randomChance;

    public Gradient gradient;
    public Vector2 gradientStart;
    public Vector2 gradientEnd;
    public enum GradientRepeatType { Clamp, Repeat, Reflect }
    public GradientRepeatType gradientRepeat;

    public Vector2 tileSizeOverride = Vector2.zero;

    public GameObject GetVariantAtPosition(Vector2 tileCoord)
    {
        float r = 0;
        switch (variantPatternType)
        {
            case VariantPatternType.Image:
                Color c = image.GetPixel(Mathf.RoundToInt(tileCoord.x), Mathf.RoundToInt(tileCoord.y));
                r = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
                break;
            case VariantPatternType.Perlin:
                r = Mathf.PerlinNoise(perlinParameters.x * tileCoord.x, perlinParameters.y * tileCoord.y);
                break;
            case VariantPatternType.Random:
                if (Random.value >= randomChance) { return null; }
                else { r = Random.value; }
                break;
            case VariantPatternType.Gradient:
                r = 0.5f;
                break;
        }
        r = Mathf.Clamp(r, 0.0001f, 0.9999f);
        return variants[Mathf.FloorToInt(r * variants.Length)];
    }
}
