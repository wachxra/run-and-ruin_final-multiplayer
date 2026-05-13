using UnityEngine;

public class SkillVFXDatabase : MonoBehaviour
{
    public static SkillVFXDatabase Instance;

    [Header("Runner - StopTime")]
    public GameObject stopTimeVFX;
    public float stopTimeScale = 5f;

    [Header("Runner - ScreenBlock")]
    public GameObject screenBlockVFX;
    public float screenBlockScale = 5f;
    public GameObject screenBlockLaneLoopVFX;
    public float screenBlockLaneScale = 2f;

    [Header("Runner - ScreenBlock Lane Points")]
    public Transform[] screenBlockLanePoints;

    [Header("Runner - Reflect")]
    public GameObject reflectActiveVFX;
    public float reflectActiveScale = 5f;
    public GameObject reflectHitVFX;
    public float reflectHitScale = 5f;

    [Header("Runner - Shield")]
    public GameObject shieldActiveVFX;
    public float shieldActiveScale = 1f;

    public GameObject shieldBreakVFX;
    public float shieldBreakScale = 5f;

    [Header("Runner - Invisible")]
    public GameObject invisibleStartVFX;
    public float invisibleScale = 5f;
    public GameObject invisibleEndVFX;

    [Header("Runner - Slow Status")]
    public GameObject slowActiveVFX;
    public float slowScale = 5f;


    private void Awake()
    {
        Instance = this;
    }
}