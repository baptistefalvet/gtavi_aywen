using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effects
{
    public class FogPosterization : VolumeComponent, IPostProcessComponent
    {
        //Color Clamping
        public FloatParameter MinClamping = new FloatParameter(0.25f);
        public FloatParameter MaxClamping = new FloatParameter(2.0f);
        public IntParameter Steps = new IntParameter(8);

        public Vector2Parameter Remaping = new Vector2Parameter(Vector2.zero);

        public BoolParameter Clamping = new BoolParameter(false);

        //INTERFACE REQUIREMENT 
        public bool IsActive() => true;
        public bool IsTileCompatible() => false;
    }
}