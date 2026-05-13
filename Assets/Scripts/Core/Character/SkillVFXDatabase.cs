using UnityEngine;

public class SkillVFXDatabase : MonoBehaviour
{
    public static SkillVFXDatabase Instance;

    [Header("Runner VFX")]
    public GameObject stopTimeVFX;
    public GameObject screenBlockVFX;
    public GameObject reflectActiveVFX;
    public GameObject reflectHitVFX;
    public GameObject shieldActiveVFX;
    public GameObject shieldBreakVFX;
    public GameObject invisibleStartVFX;
    public GameObject invisibleEndVFX;
    public GameObject slowActiveVFX;

    [Header("Trickster VFX")]
    public GameObject cannonCastVFX;
    public GameObject blurCastVFX;
    public GameObject scientistSlowVFX;
    public GameObject rapidFireVFX;
    public GameObject shadowHideVFX;

    private void Awake()
    {
        Instance = this;
    }
}