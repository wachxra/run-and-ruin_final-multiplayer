using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill Database")]
public class CharacterSkillDatabase : ScriptableObject
{
    [Header("Runner Skills")]
    public CharacterSkillSO[] runnerSkills;

    [Header("Trickster Skills")]
    public CharacterSkillSO[] tricksterSkills;
}