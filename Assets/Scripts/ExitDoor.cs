using System;
using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Escape Condition")]
    [SerializeField] private int requiredPasswordCount = 3;

    [Header("References")]
    [SerializeField] private QuizManager quizManager;

    [Header("Messages")]
    [SerializeField] private string lockedMessage = "아직 비밀번호를 다 모으지 못했습니다.";
    [SerializeField] private string escapeMessage = "탈출 성공! 교실을 빠져나왔습니다.";

    public bool HasEscaped { get; private set; }

    // 문은 연필로 치지 않고 그냥 열리므로 타격 모션이 필요 없음
    public bool PlaysHitMotion => false;

    private bool isBusy;

    private void Awake()
    {
        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<QuizManager>();
        }
    }

    // PlayerDeskInteractor가 감지 후 호출하는 상호작용 진입점
    public void Interact()
    {
        if (HasEscaped || isBusy)
        {
            return;
        }

        TryEscape();
    }

    private void TryEscape()
    {
        if (PasswordStorage.Instance == null)
        {
            Debug.LogError("PasswordStorage가 씬에 없습니다.");
            return;
        }

        isBusy = true;

        int collectedCount = PasswordStorage.Instance.GetPasswordCount();

        if (collectedCount < requiredPasswordCount)
        {
            ShowMessage(lockedMessage, () => isBusy = false);
            return;
        }

        ShowMessage(escapeMessage, OnEscapeSuccess);
    }

    private void OnEscapeSuccess()
    {
        HasEscaped = true;
        isBusy = false;

        // 게임 클리어 처리 확장 지점 (엔딩 UI, 씬 전환 등을 여기에 연결)
        Debug.Log("Game Clear!");
    }

    private void ShowMessage(string message, Action onClosed)
    {
        if (quizManager != null)
        {
            quizManager.ShowMessageOnly(message, onClosed);
        }
        else
        {
            Debug.LogWarning("QuizManager가 연결되지 않아 메시지를 콘솔로만 출력합니다: " + message);
            onClosed?.Invoke();
        }
    }
}
