using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PasswordStorage : MonoBehaviour
{
    public static PasswordStorage Instance { get; private set; }

    [SerializeField] private TMP_Text passwordText;

    private List<string> passwords = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateUI();
    }

    public void AddPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        passwords.Add(password.Trim());
        UpdateUI();
    }

    // 지금까지 모은 비밀번호 조각 개수 (탈출문에서 조건 확인용)
    public int GetPasswordCount()
    {
        return passwords.Count;
    }

    // 조각을 모은 순서대로 이어 붙인 전체 비밀번호 (필요 시 확장용)
    public string GetCombinedPassword()
    {
        return string.Join("", passwords);
    }

    private void UpdateUI()
    {
        if (passwordText == null)
        {
            return;
        }

        if (passwords.Count == 0)
        {
            passwordText.text = "비밀번호:";
        }
        else
        {
            passwordText.text = "비밀번호: " + string.Join(" ", passwords);
        }
    }
}
