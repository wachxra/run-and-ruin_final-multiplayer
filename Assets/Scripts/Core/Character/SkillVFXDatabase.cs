using UnityEngine;

public class SkillVFXDatabase : MonoBehaviour
{
    public static SkillVFXDatabase Instance;

    [Header("Runner - StopTime")]
    public GameObject stopTimeVFX;

    [Header("Runner - ScreenBlock")]
    public GameObject screenBlockVFX;
    public GameObject screenBlockLaneLoopVFX;

    [Header("Runner - ScreenBlock Lane Points")]
    public Transform[] screenBlockLanePoints;

    [Header("Runner - Reflect")]
    public GameObject reflectActiveVFX;
    public GameObject reflectHitVFX;

    [Header("Runner - Shield")]
    public GameObject shieldActiveVFX;
    public GameObject shieldBreakVFX;

    [Header("Runner - Invisible")]
    public GameObject invisibleStartVFX;
    public GameObject invisibleEndVFX;

    [Header("Runner - Slow Status")]
    public GameObject slowActiveVFX;

    private void Awake()
    {
        Instance = this;
    }
}