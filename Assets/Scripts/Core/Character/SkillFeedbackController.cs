using UnityEngine;
using Unity.Netcode;

public class SkillFeedbackController : NetworkBehaviour
{
    [Header("Runner VFX")]
    public GameObject shieldActiveVFX;
    public GameObject shieldBreakVFX;
    public GameObject reflectActiveVFX;
    public GameObject reflectHitVFX;
    public GameObject slowActiveVFX;
    public GameObject invisibleStartVFX;
    public GameObject invisibleEndVFX;

    [Header("General Skill VFX")]
    public GameObject stopTimeVFX;
    public GameObject screenBlockVFX;

    [Header("Trickster VFX")]
    public GameObject cannonCastVFX;
    public GameObject blurCastVFX;
    public GameObject rapidFireVFX;
    public GameObject shadowHideVFX;
    public GameObject scientistSlowVFX;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shieldOnSFX;
    public AudioClip shieldBreakSFX;
    public AudioClip reflectSFX;
    public AudioClip slowSFX;
    public AudioClip invisibleSFX;
    public AudioClip skillCastSFX;

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
        PlayOneShotWorldVFXClientRpc(SkillFeedbackType.ReflectHit, position);
    }

    public void PlaySlowStart(float duration)
    {
        PlaySlowStartClientRpc(duration);
    }

    public void PlaySlowEnd()
    {
        PlaySlowEndClientRpc();
    }

    public void PlayTricksterSkillStart(SkillType skillType, float duration)
    {
        PlayTricksterSkillStartClientRpc(skillType, duration);
    }

    public void PlayProjectileHide(Vector3 position)
    {
        PlayOneShotWorldVFXClientRpc(SkillFeedbackType.ShadowHide, position);
    }

    [ClientRpc]
    void PlayRunnerSkillStartClientRpc(SkillType skillType, float duration)
    {
        switch (skillType)
        {
            case SkillType.StopTime:
                SpawnOneShot(stopTimeVFX, transform.position);
                PlaySFX(skillCastSFX);
                break;

            case SkillType.ScreenBlock:
                SpawnOneShot(screenBlockVFX, transform.position);
                PlaySFX(skillCastSFX);
                break;

            case SkillType.Reflect:
                currentReflectVFX = SpawnLoop(reflectActiveVFX);
                PlaySFX(reflectSFX);
                break;

            case SkillType.Shield:
                currentShieldVFX = SpawnLoop(shieldActiveVFX);
                PlaySFX(shieldOnSFX);
                break;

            case SkillType.Invisible:
                SpawnOneShot(invisibleStartVFX, transform.position);
                PlaySFX(invisibleSFX);
                break;
        }
    }

    [ClientRpc]
    void PlayRunnerSkillEndClientRpc(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Reflect:
                DestroyCurrent(ref currentReflectVFX);
                break;

            case SkillType.Shield:
                DestroyCurrent(ref currentShieldVFX);
                break;

            case SkillType.Invisible:
                SpawnOneShot(invisibleEndVFX, transform.position);
                break;
        }
    }

    [ClientRpc]
    void PlayShieldBreakClientRpc()
    {
        DestroyCurrent(ref currentShieldVFX);

        SpawnOneShot(shieldBreakVFX, transform.position);

        PlaySFX(shieldBreakSFX);
    }

    [ClientRpc]
    void PlaySlowStartClientRpc(float duration)
    {
        DestroyCurrent(ref currentSlowVFX);

        currentSlowVFX = SpawnLoop(slowActiveVFX);

        PlaySFX(slowSFX);
    }

    [ClientRpc]
    void PlaySlowEndClientRpc()
    {
        DestroyCurrent(ref currentSlowVFX);
    }

    [ClientRpc]
    void PlayTricksterSkillStartClientRpc(SkillType skillType, float duration)
    {
        switch (skillType)
        {
            case SkillType.Cannon:
                SpawnOneShot(cannonCastVFX, transform.position);
                PlaySFX(skillCastSFX);
                break;

            case SkillType.Blur:
                SpawnOneShot(blurCastVFX, transform.position);
                PlaySFX(skillCastSFX);
                break;

            case SkillType.SlowAll:
                SpawnOneShot(scientistSlowVFX, transform.position);
                PlaySFX(slowSFX);
                break;

            case SkillType.MultiShot:
                currentRapidFireVFX = SpawnLoop(rapidFireVFX);
                PlaySFX(skillCastSFX);
                Invoke(nameof(StopRapidFireVFX), duration);
                break;

            case SkillType.Trap:
                SpawnOneShot(shadowHideVFX, transform.position);
                PlaySFX(skillCastSFX);
                break;
        }
    }

    void StopRapidFireVFX()
    {
        DestroyCurrent(ref currentRapidFireVFX);
    }

    [ClientRpc]
    void PlayOneShotWorldVFXClientRpc(SkillFeedbackType feedbackType, Vector3 position)
    {
        if (feedbackType == SkillFeedbackType.ReflectHit)
        {
            SpawnOneShot(reflectHitVFX, position);
            PlaySFX(reflectSFX);
        }
        else if (feedbackType == SkillFeedbackType.ShadowHide)
        {
            SpawnOneShot(shadowHideVFX, position);
        }
    }

    GameObject SpawnLoop(GameObject prefab)
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity,
            transform);

        return obj;
    }

    void SpawnOneShot(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        Instantiate(
            prefab,
            position,
            Quaternion.identity);
    }

    void DestroyCurrent(ref GameObject obj)
    {
        if (obj == null) return;

        Destroy(obj);

        obj = null;
    }

    void PlaySFX(AudioClip clip)
    {
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}

public enum SkillFeedbackType
{
    ReflectHit,
    ShadowHide
}