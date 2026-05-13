using UnityEngine;
using Unity.Netcode;

public class SkillFeedbackController : NetworkBehaviour
{
    private GameObject currentShieldVFX;
    private GameObject currentReflectVFX;
    private GameObject currentSlowVFX;
    private GameObject currentRapidFireVFX;

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
        PlayOneShotWorldVFXClientRpc(
            SkillFeedbackType.ReflectHit,
            position);
    }

    public void PlaySlowStart(float duration)
    {
        PlaySlowStartClientRpc(duration);
    }

    public void PlaySlowEnd()
    {
        PlaySlowEndClientRpc();
    }

    public void PlayTricksterSkillStart(
        SkillType skillType,
        float duration)
    {
        PlayTricksterSkillStartClientRpc(
            skillType,
            duration);
    }

    public void PlayProjectileHide(Vector3 position)
    {
        PlayOneShotWorldVFXClientRpc(
            SkillFeedbackType.ShadowHide,
            position);
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

                SpawnOneShot(
                    SkillVFXDatabase.Instance.stopTimeVFX,
                    transform.position);

                break;

            case SkillType.ScreenBlock:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.screenBlockVFX,
                    transform.position);

                break;

            case SkillType.Reflect:

                currentReflectVFX =
                    SpawnLoop(
                        SkillVFXDatabase.Instance.reflectActiveVFX);

                break;

            case SkillType.Shield:

                currentShieldVFX =
                    SpawnLoop(
                        SkillVFXDatabase.Instance.shieldActiveVFX);

                break;

            case SkillType.Invisible:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.invisibleStartVFX,
                    transform.position);

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

                SpawnOneShot(
                    SkillVFXDatabase.Instance.invisibleEndVFX,
                    transform.position);

                break;
        }
    }

    [ClientRpc]
    void PlayShieldBreakClientRpc()
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        DestroyCurrent(ref currentShieldVFX);

        SpawnOneShot(
            SkillVFXDatabase.Instance.shieldBreakVFX,
            transform.position);
    }

    [ClientRpc]
    void PlaySlowStartClientRpc(float duration)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        DestroyCurrent(ref currentSlowVFX);

        currentSlowVFX =
            SpawnLoop(
                SkillVFXDatabase.Instance.slowActiveVFX);
    }

    [ClientRpc]
    void PlaySlowEndClientRpc()
    {
        DestroyCurrent(ref currentSlowVFX);
    }

    [ClientRpc]
    void PlayTricksterSkillStartClientRpc(
        SkillType skillType,
        float duration)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        switch (skillType)
        {
            case SkillType.Cannon:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.cannonCastVFX,
                    transform.position);

                break;

            case SkillType.Blur:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.blurCastVFX,
                    transform.position);

                break;

            case SkillType.SlowAll:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.scientistSlowVFX,
                    transform.position);

                break;

            case SkillType.MultiShot:

                currentRapidFireVFX =
                    SpawnLoop(
                        SkillVFXDatabase.Instance.rapidFireVFX);

                Invoke(
                    nameof(StopRapidFireVFX),
                    duration);

                break;

            case SkillType.Trap:

                SpawnOneShot(
                    SkillVFXDatabase.Instance.shadowHideVFX,
                    transform.position);

                break;
        }
    }

    void StopRapidFireVFX()
    {
        DestroyCurrent(ref currentRapidFireVFX);
    }

    [ClientRpc]
    void PlayOneShotWorldVFXClientRpc(
        SkillFeedbackType feedbackType,
        Vector3 position)
    {
        if (SkillVFXDatabase.Instance == null)
            return;

        if (feedbackType == SkillFeedbackType.ReflectHit)
        {
            SpawnOneShot(
                SkillVFXDatabase.Instance.reflectHitVFX,
                position);
        }
        else if (feedbackType == SkillFeedbackType.ShadowHide)
        {
            SpawnOneShot(
                SkillVFXDatabase.Instance.shadowHideVFX,
                position);
        }
    }

    GameObject SpawnLoop(GameObject prefab)
    {
        if (prefab == null)
            return null;

        GameObject obj = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity,
            transform);

        return obj;
    }

    void SpawnOneShot(
        GameObject prefab,
        Vector3 position)
    {
        if (prefab == null)
            return;

        Instantiate(
            prefab,
            position,
            Quaternion.identity);
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