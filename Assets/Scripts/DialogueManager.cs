using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Ink.Runtime;
using Ink;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueArrow;
    public GameObject nameBox;

    [Header("Select UI")]
    public GameObject choicePanel;
    public GameObject choiceBlocker;
    public GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Main Character Info")]
    public GameObject mainCharacter;
    public string characterName;

    [Header("Ink JSON Files")]
    public TextAsset initialDialogue;
    public TextAsset characterWinDialogue;
    public TextAsset characterLoseDialogue;

    private Story currentStory;
    private static DialogueManager instance;
    [HideInInspector] public int characterStatus; // 0 = 초기화, 1 = 이니셜 대화, 2 = Win, 3 = Lose
    private bool isDialoguePlaying;

    private void Awake() 
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            //Debug.LogWarning("씬에 한 개 이상의 Dialogue Manager가 있습니다.");
        }
        else
        {
            instance = this;
        }
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start() 
    {
        dialoguePanel.SetActive(false);
        nameText.text = null;
        dialogueText.text = null;
        dialogueArrow.SetActive(false);
        isDialoguePlaying = false;

        choicePanel.SetActive(false);
        choiceBlocker.SetActive(false);
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update() 
    {
        if (!isDialoguePlaying)
        {
            return;
        }

        if (characterStatus == 1)
        {
            StartDialogue(initialDialogue);
        }
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        isDialoguePlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    public void ContinueStory()
    {
        if(currentStory.canContinue)
        {
            string rawText = currentStory.Continue();
            ParseDialogueText(rawText);
            DisplayChoice();
            // dialogueText.text = currentStory.Continue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ParseDialogueText(string rawText)
    {
        string[] parts = rawText.Split(':'); // ":"를 기준으로 문자열을 분리

        if (parts.Length == 2)
        {
            switch (parts[0])
            {    
                case("c"):
                    parts[0] = parts[0].Replace("c", characterName);
                    nameBox.SetActive(true);
                    nameText.text = parts[0];
                    break;
                case("p"):
                    parts[0] = parts[0].Replace("p", "");
                    nameBox.SetActive(false);
                    nameText.text = parts[0];
                    break;
                default:
                    Debug.LogError("잘못된 형식의 대화입니다: " + rawText);
                    break;
            }

            dialogueText.text = parts[1];
        }
        else
        {
            Debug.LogError("잘못된 형식의 대화입니다: " + rawText);
        }
    }

    private void EndDialogue()
    {
        isDialoguePlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = null;
        characterStatus = 0;
    }

    private void DisplayChoice()
    {
        List<Choice> currentChoice = currentStory.currentChoices;

        if (currentChoice.Count > choices.Length)
        {
            Debug.LogError("선택지 UI 개수보다 더 많은 선택지가 있습니다.");
        }

        int index = 0;
        
        // 이 대화 라인에 대한 선택의 양까지 선택을 활성화하고 초기화
        foreach(Choice choice in currentChoice)
        {
            choicePanel.SetActive(true);
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        // UI가 지원하는 나머지 선택 사항을 살펴보고 숨겨진 항목을 확인합니다
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        //StartCoroutine(SelectFirstChoice());
    }

    //private IEnumerator SelectFirstChoice() // 첫번째 선택지 자동으로 선택하게 하는 함수
    //{
    //    // Event System은 현재 선택된 오브젝트를 설정하기 적어도 한 프레임 전에 초기화하고 대기해야 함
    //    EventSystem.current.SetSelectedGameObject(null);
    //    yield return new WaitForEndOfFrame();
    //    EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    //}

    public void MakeChoice(int choinceIndex)
    {
        currentStory.ChooseChoiceIndex(choinceIndex);
        choicePanel.SetActive(false);
        choiceBlocker.SetActive(false);
    }
}