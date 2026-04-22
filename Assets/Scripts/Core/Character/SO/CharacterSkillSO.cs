using UnityEngine;

public enum SkillType
{
    StopTime,
    ScreenBlock,
    Reflect,
    Shield,
    Invisible,

    Cannon,
    Blur,
    SlowAll,
    MultiShot,
    Trap
}

[CreateAssetMenu(menuName = "Game/Character Skill")]
public class CharacterSkillSO : ScriptableObject
{
    public string skillName;
    public SkillType skillType;
    public float cooldown = 5f;
    public float duration = 5f;

    [Header("UI")]
    public Sprite icon;
}