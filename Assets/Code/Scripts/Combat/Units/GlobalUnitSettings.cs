using UnityEngine;

[SingletonMode(true)]
public class GlobalUnitSettings : Singleton<GlobalUnitSettings>
{
    public Material HurtMaterial;
    public Material HealMaterial;
    public float MaterialEffectDuration;
}
