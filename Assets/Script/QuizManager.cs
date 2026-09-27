using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        [TextArea(2, 4)] public string questionText;
        public string[] answers = new string[4];
        public int correctIndex;
    }

    [Header("UI References (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionTextUI;
    [SerializeField] private TextMeshProUGUI progressTextUI;
    [SerializeField] private Button[] answerButtons;

    [Header("UI References (Result Panel)")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI scoreTextUI;
    [SerializeField] private TextMeshProUGUI feedbackTextUI;

    [Header("Feedback Color Settings")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private float feedbackDelay = 0.5f;

    [Header("Questions Repository")]
    [SerializeField] private List<QuestionData> masterQuestionList = new List<QuestionData>();

    private List<QuestionData> dynamicQuizList = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private bool isProcessingAnswer = false; // Prevents double-clicking during delay
    private Color[] originalButtonColors;

    void Start()
    {
        // 1. FORCE RESET COUNTERS: Fixes tracking issues when moving between scenes
        currentQuestionIndex = 0;
        score = 0;
        isProcessingAnswer = false;

        // 2. Reset button click configuration paths completely
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.RemoveAllListeners();
            }
        }

        quizPanel.SetActive(true);
        resultPanel.SetActive(false);

        originalButtonColors = new Color[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            originalButtonColors[i] = answerButtons[i].GetComponent<Image>().color;
        }

        dynamicQuizList = new List<QuestionData>(masterQuestionList);
        ShuffleQuestions(dynamicQuizList);

        DisplayQuestion();
    }



    private void ShuffleQuestions(List<QuestionData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            QuestionData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void DisplayQuestion()
    {
        if (currentQuestionIndex >= dynamicQuizList.Count)
        {
            ShowResults();
            return;
        }

        // Reset button visual colors and enable click interactions
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponent<Image>().color = originalButtonColors[i];
            answerButtons[i].interactable = true;
        }

        QuestionData currentQuestion = dynamicQuizList[currentQuestionIndex];
        questionTextUI.text = currentQuestion.questionText;
        progressTextUI.text = $"Question {currentQuestionIndex + 1} / {dynamicQuizList.Count}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = currentQuestion.answers[i];

            answerButtons[i].onClick.RemoveAllListeners();
            int selectedIndex = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerClick(selectedIndex));
        }

        isProcessingAnswer = false;
    }

    private void OnAnswerClick(int index)
    {
        if (isProcessingAnswer) return; // Ignore input if already flashing
        StartCoroutine(ProcessAnswerRoutine(index));
    }

    private IEnumerator ProcessAnswerRoutine(int index)
    {
        isProcessingAnswer = true;

        // Temporarily disable all buttons so user cannot click others during flash sequence
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        int correctIdx = dynamicQuizList[currentQuestionIndex].correctIndex;

        if (index == correctIdx)
        {
            score++;
            answerButtons[index].GetComponent<Image>().color = correctColor;
        }
        else
        {
            // Flash clicked button Red and the correct choice Green
            answerButtons[index].GetComponent<Image>().color = incorrectColor;
            answerButtons[correctIdx].GetComponent<Image>().color = correctColor;
        }

        // Wait for the specified delay duration (0.5s)
        yield return new WaitForSeconds(feedbackDelay);

        currentQuestionIndex++;
        DisplayQuestion();
    }

    private void ShowResults()
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);

        int totalQuestions = dynamicQuizList.Count;
        scoreTextUI.text = $"Total Score: {score} / {totalQuestions}";

        float percentage = ((float)score / totalQuestions) * 100f;
        string feedback = "";

        if (percentage >= 90f) feedback = "Excellent! You understood the alkali metals well.";
        else if (percentage >= 70f) feedback = "Good job! Small improvements needed.";
        else if (percentage >= 50f) feedback = "Fair. Continue to review the concepts.";
        else feedback = "Please revisit the learning content.";

        feedbackTextUI.text = feedback;
    }

    public void OnClickRetryQuiz()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(currentSceneName);
    }

    public void OnClickMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

}
