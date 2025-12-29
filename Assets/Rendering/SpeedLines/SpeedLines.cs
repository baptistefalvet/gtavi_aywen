using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effects
{
    public class SpeedLines : VolumeComponent, IPostProcessComponent
    {
        //PIXELATION
        public FloatParameter EffectRaduis = new FloatParameter(20f);
        public FloatParameter EffectSpeed = new FloatParameter(1f);
        public FloatParameter LinesEdges = new FloatParameter(0.25f);

        public Vector2Parameter RedOffset = new Vector2Parameter(Vector2.zero);
        public Vector2Parameter GreenOffset = new Vector2Parameter(Vector2.zero);
        public Vector2Parameter BlueOffset = new Vector2Parameter(Vector2.zero);

        //COLOR PRECISION 
        public ColorParameter LinesColor = new ColorParameter(Color.black,true,false,true);

        //INTERFACE REQUIREMENT 
        public bool IsActive() => true;
        public bool IsTileCompatible() => false;
    }
}