using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenBlurEffect : MonoBehaviour
{
    public Image blurImage;

    public void PlayBlur(float duration)
    {
        StartCoroutine(BlurRoutine(duration));
    }

    IEnumerator BlurRoutine(float duration)
    {
        if (blurImage == null)
            yield break;

        blurImage.gameObject.SetActive(true);

        Color c = blurImage.color;

        c.a = 0.9f;

        blurImage.color = c;

        yield return new WaitForSeconds(duration);

        c.a = 0f;

        blurImage.color = c;

        blurImage.gameObject.SetActive(false);
    }
}