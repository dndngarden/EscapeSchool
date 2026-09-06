using UnityEngine;

public class DeskPuzzle : MonoBehaviour, IInteractable
{
    // 책상은 문제/규칙과 상관없이 항상 연필로 치는 모션을 먼저 재생한다
    public bool PlaysHitMotion => true;

    [Header("Desk Sprite")]
    [SerializeField] private Sprite brokenSprite;   // 규칙 책상 잔해 이미지: desk_destroyed 연결

    [Header("Collider")]
    [SerializeField] private Collider2D obstacleCollider;

    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;

    private QuizQuestion assignedQuestion;
    private string assignedRuleMessage;

    private bool hasPuzzle;
    private bool solved;
    private bool isBusy;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            normalSprite = spriteRenderer.sprite;
        }
        else
        {
            Debug.LogWarning(name + " 책상에서 SpriteRenderer를 찾지 못했습니다.");
        }

        if (obstacleCollider == null)
        {
            obstacleCollider = GetComponent<Collider2D>();
        }

        if (obstacleCollider == null)
        {
            obstacleCollider = GetComponentInChildren<Collider2D>();
        }
    }

    public void SetPuzzle(QuizQuestion question)
    {
        assignedQuestion = question;
        assignedRuleMessage = "";
        hasPuzzle = question != null;
        solved = false;
        isBusy = false;

        RestoreDesk();
    }

    public void SetRuleMessage(string ruleMessage)
    {
        assignedQuestion = null;
        assignedRuleMessage = ruleMessage;
        hasPuzzle = false;
        solved = false;
        isBusy = false;

        RestoreDesk();
    }

    public void Interact()
    {
        if (solved || isBusy)
        {
            return;
        }

        isBusy = true;

        if (hasPuzzle)
        {
            OpenQuizDesk();
        }
        else
        {
            OpenRuleDesk();
        }
    }

    private void OpenQuizDesk()
    {
        if (QuizManager.Instance == null)
        {
            Debug.LogError("QuizManager가 씬에 없습니다.");
            isBusy = false;
            return;
        }

        QuizManager.Instance.OpenQuiz(this, assignedQuestion);
    }

    private void OpenRuleDesk()
    {
        if (QuizManager.Instance == null)
        {
            Debug.LogError("QuizManager가 씬에 없습니다.");
            isBusy = false;
            return;
        }

        // 규칙 UI가 닫힌 뒤 규칙 책상은 잔해로 변경
        QuizManager.Instance.ShowMessageOnly(assignedRuleMessage, OnRuleMessageClosed);
    }

    private void OnRuleMessageClosed()
    {
        // 규칙 책상은 사라지지 않고 desk_destroyed 잔해가 남음
        LeaveDebris();
    }

    public void RestoreDesk()
    {
        if (solved)
        {
            return;
        }

        isBusy = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            if (normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
        }

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = true;
        }
    }

    // 문제 책상 정답 처리용
    // 정답을 맞히면 기존처럼 책상이 사라짐
    public void MarkSolved()
    {
        solved = true;
        isBusy = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = false;
        }
    }

    // 규칙 책상 처리용
    // 규칙을 보여준 뒤 desk_destroyed 잔해 이미지로 바뀜
    private void LeaveDebris()
    {
        solved = true;
        isBusy = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            if (brokenSprite != null)
            {
                spriteRenderer.sprite = brokenSprite;
            }
            else
            {
                Debug.LogWarning(name + "의 Broken Sprite가 비어 있습니다. desk_destroyed를 연결하세요.");
                spriteRenderer.enabled = false;
            }
        }

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = false;
        }
    }

    public void CancelBusy()
    {
        isBusy = false;
    }
}