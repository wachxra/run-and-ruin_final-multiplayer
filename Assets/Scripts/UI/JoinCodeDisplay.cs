using UnityEngine;
using TMPro; // ถ้าใช้ TextMeshPro

public class JoinCodeDisplay : MonoBehaviour
{
    public TextMeshProUGUI joinCodeText;

    private const string JoinCodeKey = "JoinCode";

    void Start()
    {
        string code = PlayerPrefs.GetString(JoinCodeKey, "----");
        joinCodeText.text = "Join Code: " + code;
    }
}