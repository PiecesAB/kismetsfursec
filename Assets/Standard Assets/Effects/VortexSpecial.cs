using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Displacement/Vortex")]
    public class VortexSpecial : ImageEffectBase
    {
        public Vector2 radius = new Vector2(0.4F,0.4F);
        public float angle = 20;
        public Vector2 center = new Vector2(0.5F, 0.5F);
		
        void Update()
        {
            angle = 20f * Mathf.Sin(0.2f*Time.time);
        }

        // Called by camera to apply image effect
        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            ImageEffects.RenderDistortion (material, source, destination, angle, center, radius);
        }
    }
}
