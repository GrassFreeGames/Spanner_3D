using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/Saturation Effect")]
public class SaturationVolume : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Saturation intensity: 0 = black and white, 1 = normal, 2 = oversaturated")]
    public ClampedFloatParameter saturation = new ClampedFloatParameter(1f, 0f, 2f);

    public bool IsActive() => saturation.value != 1f;

    public bool IsTileCompatible() => false;
}
