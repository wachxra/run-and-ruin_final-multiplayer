using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    public Image skillIcon;

    public void SetSkill(CharacterSkillSO skill)
    {
        if (skill == null) return;

        if (skillIcon != null)
            skillIcon.sprite = skill.icon;
    }
}