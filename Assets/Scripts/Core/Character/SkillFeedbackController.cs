using UnityEngine;
using Unity.Netcode;

public class SkillFeedbackController : NetworkBehaviour
{
    [Header("VFX Spawn Point")]
    public Transform runnerCenterPoint;

    private GameObject currentShieldVFX;
    private GameObject currentReflectVFX;
    private GameObject currentSlowVFX;
    private GameObject currentScreenBlockLaneVFX;

    private const float HitVFXDuration = 0.5f;
    private const float BreakVFXDuration = 0.5f;

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
                    SkillVFXDatabase.Instance.stopTimeScale,
                    duration);
                break;

            case SkillType.ScreenBlock:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.screenBlockVFX,
                    SkillVFXDatabase.Instance.screenBlockScale,
                    duration);
                break;

            case SkillType.Reflect:
                DestroyCurrent(ref currentReflectVFX);

                currentReflectVFX =
                    SpawnLoopAtRunner(
                        SkillVFXDatabase.Instance.reflectActiveVFX,
                        SkillVFXDatabase.Instance.reflectActiveScale);
                break;

            case SkillType.Shield:
                DestroyCurrent(ref currentShieldVFX);

                currentShieldVFX =
                    SpawnLoopAtRunner(
                        SkillVFXDatabase.Instance.shieldActiveVFX,
                        SkillVFXDatabase.Instance.shieldActiveScale);
                break;

            case SkillType.Invisible:
                SpawnOneShotAtRunner(
                    SkillVFXDatabase.Instance.invisibleStartVFX,
                    SkillVFXDatabase.Instance.invisibleScale,
                    duration);
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
                    SkillVFXDatabase.Instance.invisibleScale,
                    HitVFXDuration);
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
            SkillVFXDatabase.Instance.shieldBreakScale,
            BreakVFXDuration);
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
                SkillVFXDatabase.Instance.slowScale);
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
            SkillVFXDatabase.Instance.reflectHitScale,
            HitVFXDuration);
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

        currentScreenBlockLaneVFX.transform.localScale *=
            SkillVFXDatabase.Instance.screenBlockLaneScale;

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
        float scaleMultiplier,
        float duration)
    {
        if (prefab == null)
            return;

        GameObject obj = Instantiate(
            prefab,
            RunnerCenterPosition,
            Quaternion.identity);

        obj.transform.localScale *= scaleMultiplier;

        DestroyAfterAnimation destroyer =
            obj.GetComponent<DestroyAfterAnimation>();

        if (destroyer != null)
        {
            destroyer.SetDestroyTime(duration);
        }
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