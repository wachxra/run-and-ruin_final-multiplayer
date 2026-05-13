using UnityEngine;

public class SkillVFXDatabase : MonoBehaviour
{
    public static SkillVFXDatabase Instance;

    [Header("Runner - StopTime")]
    public GameObject stopTimeVFX;
    public float stopTimeScale = 4f;

    [Header("Runner - ScreenBlock")]
    public GameObject screenBlockVFX;
    public float screenBlockScale = 3f;
    public GameObject screenBlockLaneLoopVFX;
    public float screenBlockLaneScale = 5f;

    [Header("Runner - ScreenBlock Lane Points")]
    public Transform[] screenBlockLanePoints;

    [Header("Runner - Reflect")]
    public GameObject reflectActiveVFX;
    public float reflectActiveScale = 2.5f;
    public GameObject reflectHitVFX;
    public float reflectHitScale = 2f;

    [Header("Runner - Shield")]
    public GameObject shieldActiveVFX;
    public float shieldActiveScale = 3f;

    public GameObject shieldBreakVFX;
    public float shieldBreakScale = 4f;

    [Header("Runner - Invisible")]
    public GameObject invisibleStartVFX;
    public float invisibleScale = 2f;
    public GameObject invisibleEndVFX;

    [Header("Runner - Slow Status")]
    public GameObject slowActiveVFX;
    public float slowScale = 2f;


    private void Awake()
    {
        Instance = this;
    }
}