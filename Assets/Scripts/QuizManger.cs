using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("Desk Setting")]
    [SerializeField] private List<DeskPuzzle> allDesks = new List<DeskPuzzle>();
    [SerializeField] private int puzzleDeskCount = 3;

    [Header("Quiz Data")]
    [SerializeField] private List<QuizQuestion> questions = new List<QuizQuestion>();

    [Header("Classroom Rules")]
    [SerializeField] private List<string> classroomRules = new List<string>()
    {
        "정리정돈을 잘 해요.",
        "친구에게 바른 말을 사용해요.",
        "교실에서는 뛰지 않아요.",
        "차례를 지켜요.",
        "물건을 소중히 사용해요.",
        "친구의 말을 끝까지 들어요.",
        "쓰레기는 쓰레기통에 버려요."
    };

    [Header("UI")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button submitButton;

    [Header("Player Lock")]
    [SerializeField] private PlayerController playerController;

    [Header("Settings")]
    [SerializeField] private float closeDelay = 1.2f;

    public bool IsUIOpen { get; private set; }

    private QuizQuestion currentQuestion;
    private DeskPuzzle currentDesk;
    private bool isResolving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckAnswer);
        }
    }

    private void Start()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        IsUIOpen = false;
        SetPlayerLocked(false);

        AssignRandomPuzzleDesks();
    }

    private void AssignRandomPuzzleDesks()
    {
        if (allDesks.Count == 0)
        {
            Debug.LogWarning("All Desks 목록이 비어 있습니다. QuizManager에 책상들을 넣어주세요.");
            return;
        }

        if (questions.Count < puzzleDeskCount)
        {
            Debug.LogWarning("문제 수가 문제 책상 수보다 적습니다. Questions에 문제를 더 추가해주세요.");
            return;
        }

        // 1. 먼저 모든 책상을 규칙 메시지 책상으로 설정
        foreach (DeskPuzzle desk in allDesks)
        {
            if (desk != null)
            {
                desk.SetRuleMessage(GetRandomClassroomRule());
            }
        }

        // 2. 랜덤 선택을 위해 책상 목록과 문제 목록 복사
        List<DeskPuzzle> deskPool = new List<DeskPuzzle>(allDesks);
        List<QuizQuestion> questionPool = new List<QuizQuestion>(questions);

        int count = Mathf.Min(puzzleDeskCount, deskPool.Count, questionPool.Count);

        // 3. 책상 중 랜덤으로 puzzleDeskCount개를 뽑아서 문제 책상으로 변경
        for (int i = 0; i < count; i++)
        {
            int deskIndex = UnityEngine.Random.Range(0, deskPool.Count);
            DeskPuzzle selectedDesk = deskPool[deskIndex];
            deskPool.RemoveAt(deskIndex);

            int questionIndex = UnityEngine.Random.Range(0, questionPool.Count);
            QuizQuestion selectedQuestion = questionPool[questionIndex];
            questionPool.RemoveAt(questionIndex);

            selectedDesk.SetPuzzle(selectedQuestion);

            Debug.Log(selectedDesk.name + " 책상에 문제 배정: " + selectedQuestion.question);
        }
    }

    private string GetRandomClassroomRule()
    {
        if (classroomRules == null || classroomRules.Count == 0)
        {
            return "교실에서 지켜야 할 규칙을 생각해 봐요.";
        }

        int randomIndex = UnityEngine.Random.Range(0, classroomRules.Count);
        return classroomRules[randomIndex];
    }

    public void OpenQuiz(DeskPuzzle desk, QuizQuestion question)
    {
        if (question == null)
        {
            Debug.LogWarning("이 책상에는 문제가 없습니다.");
            return;
        }

        currentDesk = desk;
        currentQuestion = question;
        isResolving = false;

        // 문제 UI가 열리는 동안 학생 이동 제한
        SetUIOpen(true);

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = currentQuestion.question;
        }

        if (answerInput != null)
        {
            answerInput.gameObject.SetActive(true);
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;
        }
    }

    public void CheckAnswer()
    {
        if (isResolving)
        {
            return;
        }

        if (answerInput == null)
        {
            Debug.LogError("QuizManager의 Answer Input이 연결되지 않았습니다.");
            return;
        }

        if (currentQuestion == null || currentDesk == null)
        {
            Debug.LogWarning("현재 풀고 있는 문제가 없습니다.");
            return;
        }

        string userAnswer = Normalize(answerInput.text);
        string correctAnswer = Normalize(currentQuestion.answer);

        isResolving = true;

        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        if (userAnswer == correctAnswer)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }

    private void CorrectAnswer()
    {
        if (resultText != null)
        {
            resultText.text = "정답! 비밀번호를 획득했습니다.";
        }

        if (PasswordStorage.Instance != null)
        {
            PasswordStorage.Instance.AddPassword(currentQuestion.rewardPassword);
        }
        else
        {
            Debug.LogError("PasswordStorage가 씬에 없습니다.");
        }

        // 문제 책상은 정답을 맞혀야만 부서진 상태가 됨
        currentDesk.MarkSolved();

        StartCoroutine(CloseQuizAfterDelay());
    }

    private void WrongAnswer()
    {
        if (resultText != null)
        {
            resultText.text = "다시 풀어보세요.";
        }

        // 오답이면 책상은 원래 상태로 복구
        currentDesk.RestoreDesk();

        StartCoroutine(CloseQuizAfterDelay());
    }

    public void ShowMessageOnly(string message, Action onClosed = null)
    {
        // 규칙 UI가 열리는 동안 학생 이동 제한
        SetUIOpen(true);

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = message;
        }

        // 규칙 메시지에서는 정답 입력창 숨기기
        if (answerInput != null)
        {
            answerInput.gameObject.SetActive(false);
        }

        // 규칙 메시지에서는 제출 버튼 숨기기
        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        StartCoroutine(CloseMessageAfterDelay(onClosed));
    }

    private IEnumerator CloseMessageAfterDelay(Action onClosed)
    {
        yield return new WaitForSeconds(closeDelay);

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        // 다음 문제 UI를 위해 입력창과 버튼 다시 켜두기
        if (answerInput != null)
        {
            answerInput.gameObject.SetActive(true);
        }

        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
        }

        // 규칙 메시지가 닫힌 뒤 책상을 부서진 상태로 변경
        onClosed?.Invoke();

        // UI가 완전히 닫혔으므로 학생 이동 가능
        SetUIOpen(false);
    }

    private IEnumerator CloseQuizAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        currentQuestion = null;
        currentDesk = null;
        isResolving = false;

        // 문제 UI가 닫혔으므로 학생 이동 가능
        SetUIOpen(false);
    }

    private void SetUIOpen(bool open)
    {
        IsUIOpen = open;
        SetPlayerLocked(open);
    }

    private void SetPlayerLocked(bool locked)
    {
        if (playerController != null)
        {
            playerController.SetActionLocked(locked);
        }
    }

    private string Normalize(string text)
    {
        if (text == null)
        {
            return "";
        }

        // 앞뒤 공백 제거, 중간 공백 제거, 영어 대소문자 무시
        return text.Trim().Replace(" ", "").ToLower();
    }
}