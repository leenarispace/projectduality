using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{
    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = DialogueManager.GetInstance();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // 좌클릭을 감지합니다. 원하는 버튼으로 변경 가능합니다.
        {
            // 클릭 감지 시 DialogueManager에서 StartDialogue 함수 실행
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue(dialogueManager.initialDialogue);
            }
            else
            {
                Debug.LogWarning("DialogueManager가 참조되지 않았습니다.");
            }
        }
    }
}
