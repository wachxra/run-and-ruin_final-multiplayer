using UnityEngine;

public enum SkillType
{
    // Runner
    StopTime,
    ScreenBlock,
    Reflect,
    Shield,
    Invisible,

    // Trickster
    Cannon,     // Pirate King
    Blur,       // Bartender
    SlowAll,    // Scientist
    MultiShot,  // Speedster
    Trap        // Shadow
}

[CreateAssetMenu(menuName = "Game/Character Skill")]
public class CharacterSkillSO : ScriptableObject
{
    [Header("Info")]
    public string skillName;

    public SkillType skillType;

    [TextArea]
    public string description;

    [Header("Cooldown")]
    public float cooldown = 5f;

    public float duration = 5f;

    [Header("UI")]
    public Sprite icon;

    [Header("VFX")]
    public GameObject castVFX;

    public GameObject hitVFX;
}