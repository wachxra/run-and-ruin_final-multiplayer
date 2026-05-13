using UnityEngine;
using Unity.Netcode;

public class SkillFeedbackController : NetworkBehaviour
{
    [Header("VFX Spawn Point")]
    public Transform runnerCenterPoint;

    [Header("VFX Scale")]
    public float stopTimeScale = 4f;
    public float screenBlockScale = 3f;
    public float screenBlockLaneScale = 5f;
    public float reflectActiveScale = 2.5f;
    public float reflectHitScale = 2f;
    public float shieldActiveScale = 3f;
    public float shieldBreakScale = 4f;
    public float invisibleScale = 2f;
    public float slowScale = 2f;

    private GameObject currentShieldVFX;
    private GameObject currentReflectVFX;
    private GameObject currentSlowVFX;
    private GameObject currentScreenBlockLaneVFX;

    Vector3 RunnerCenterPosition
    {
        get
        {
            if (runnerCenterPoint != null)
                return runnerCenterPoint.position;

            return transform.position;
        }
    }

    Transform RunnerCenterParent
    {
        get
        {
            if (runnerCenterPoint != null)
                return runnerCenterPoint;

            return transform;
        }
    }

    public void PlayRunnerSkillStart(SkillType skillType, float duration)
    {
        PlayRunnerSkillStartClientRpc(skillType, duration);
    }

    public void PlayRunnerSkillEnd(SkillType skillType)
    {
        PlayRunnerSkillEndClientRpc(skillType);
    }

    public void PlayShieldBreak()
    {
        PlayShieldBreakClientRpc();
    }

    public void PlayReflectHit(Vector3 position)
    {
        PlayReflectHitClientRpc();
    }

    public void PlaySlowStart(float duration)
    {
        PlaySlowStartClientRpc(duration);
    }

    public void PlaySlowEnd()
    {
        PlaySlowEndClientRpc();
    }

    public void PlayScreenBlockLane(int lane, float duration)
    {
        PlayScreenBlockLaneClientRpc(lane, duration);
    }

    [ClientRpc]
    void PlayRunnerSkillStartClientRpc(
        SkillType skillType,
        float duration)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        switch (skillType)
        {
            case SkillType.StopTime:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.stopTimeVFX,
                    stopTimeScale);
                break;

            case SkillType.ScreenBlock:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.screenBlockVFX,
                    screenBlockScale);
                break;

            case SkillType.Reflect:
                DestroyCurrent(ref currentReflectVFX);

                currentReflectVFX =
                    SpawnLoopAtRunner(
                        SkillVFXDatabase.Instance.reflectActiveVFX,
                        reflectActiveScale);
                break;

            case SkillType.Shield:
                DestroyCurrent(ref currentShieldVFX);

                currentShieldVFX =
                    SpawnLoopAtRunner(
                        SkillVFXDatabase.Instance.shieldActiveVFX,
                        shieldActiveScale);
                break;

            case SkillType.Invisible:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.invisibleStartVFX,
                    invisibleScale);
                break;
        }
    }

    [ClientRpc]
    void PlayRunnerSkillEndClientRpc(SkillType skillType)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        switch (skillType)
        {
            case SkillType.Reflect:
                DestroyCurrent(ref currentReflectVFX);
                break;

            case SkillType.Shield:
                DestroyCurrent(ref currentShieldVFX);
                break;

            case SkillType.Invisible:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.invisibleEndVFX,
                    invisibleScale);
                break;
        }
    }

    [ClientRpc]
    void PlayShieldBreakClientRpc()
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        DestroyCurrent(ref currentShieldVFX);

        SpawnOneShotAtRunner(
            SkillVFXDatabase.Instance.shieldBreakVFX,
            shieldBreakScale);
    }

    [ClientRpc]
    void PlaySlowStartClientRpc(float duration)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        DestroyCurrent(ref currentSlowVFX);

        currentSlowVFX =
            SpawnLoopAtRunner(
                SkillVFXDatabase.Instance.slowActiveVFX,
                slowScale);
    }

    [ClientRpc]
    void PlaySlowEndClientRpc()
    {
        DestroyCurrent(ref currentSlowVFX);
    }

    [ClientRpc]
    void PlayReflectHitClientRpc()
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        SpawnOneShotAtRunner(
            SkillVFXDatabase.Instance.reflectHitVFX,
            reflectHitScale);
    }

    [ClientRpc]
    void PlayScreenBlockLaneClientRpc(int lane, float duration)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        if (SkillVFXDatabase.Instance.screenBlockLanePoints == null)
            return;

        int index = lane - 1;

        if (index < 0 ||
            index >= SkillVFXDatabase.Instance.screenBlockLanePoints.Length)
            return;

        Transform point =
            SkillVFXDatabase.Instance.screenBlockLanePoints[index];

        if (point == null)
            return;

        DestroyCurrent(ref currentScreenBlockLaneVFX);

        currentScreenBlockLaneVFX = Instantiate(
            SkillVFXDatabase.Instance.screenBlockLaneLoopVFX,
            point.position,
            Quaternion.identity);

        currentScreenBlockLaneVFX.transform.localScale *= screenBlockLaneScale;

        Invoke(nameof(StopScreenBlockLaneVFX), duration);
    }

    void StopScreenBlockLaneVFX()
    {
        DestroyCurrent(ref currentScreenBlockLaneVFX);
    }

    GameObject SpawnLoopAtRunner(
        GameObject prefab,
        float scaleMultiplier)
    {
        if (prefab == null)
            return null;

        GameObject obj = Instantiate(
            prefab,
            RunnerCenterPosition,
            Quaternion.identity,
            RunnerCenterParent);

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale *= scaleMultiplier;

        return obj;
    }

    void SpawnOneShotAtRunner(
        GameObject prefab,
        float scaleMultiplier)
    {
        if (prefab == null)
            return;

        GameObject obj = Instantiate(
            prefab,
            RunnerCenterPosition,
            Quaternion.identity);

        obj.transform.localScale *= scaleMultiplier;
    }

    void DestroyCurrent(ref GameObject obj)
    {
        if (obj == null)
            return;

        Destroy(obj);

        obj = null;
    }
}

public enum SkillFeedbackType
{
    ReflectHit,
    ShadowHide
}