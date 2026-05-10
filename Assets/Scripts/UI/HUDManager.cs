using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("HUD Root")]
    public GameObject hudRoot;

    [Header("References")]
    public SkillUIController skillUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        FindSkillUI();
    }

    private void FindSkillUI()
    {
        if (skillUI == null)
        {
            skillUI = FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);
        }
    }

    public void ShowHUD(bool show)
    {
        FindSkillUI();

        if (hudRoot != null)
        {
            hudRoot.SetActive(show);
        }

        if (show)
        {
            FindSkillUI();

            if (skillUI != null)
            {
                skillUI.RefreshSkillIcon();
                skillUI.RefreshCurrentSkill();
            }
        }
    }
}