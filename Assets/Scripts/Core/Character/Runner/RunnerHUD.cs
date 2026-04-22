using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunnerHUD : MonoBehaviour
{
    public static RunnerHUD Instance;

    public Image[] heartIcons;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public TMP_Text timerText;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowHUD(bool show)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = show ? 1 : 0;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
    }

    public void SetHearts(int current, int max)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (i < max)
            {
                heartIcons[i].gameObject.SetActive(true);
                heartIcons[i].sprite = i < current ? fullHeart : emptyHeart;
            }
            else
            {
                heartIcons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetTimer(float time)
    {
        if (timerText != null)
            timerText.text = time.ToString("F2") + "s";
    }

    private void Update()
    {
        if (GameFlowManager.Instance == null) return;

        SetTimer(GameFlowManager.Instance.networkTimer.Value);
    }
}