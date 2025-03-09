using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Threading;

public class ChoiceButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ChoicesAnimation(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ChoicesAnimation(false));
    }

    //private void OnMouseDown() 
    //{
    //    ChoicesClickAnimation();
    //}

    private IEnumerator ChoicesAnimation(bool hover)
    {
        if (hover == true)
        {
            ChoicesSelectAnimation();
        }
        else
        {
            ChoicesDeselectAnimation();
        }
        yield break;
    }

    private void ChoicesSelectAnimation()
    {
        rectTransform.sizeDelta = new Vector2(800, 100);
        rectTransform.DOSizeDelta(new Vector2(816, 102), 0.5f, false).SetEase(Ease.OutExpo);
    }

    private void ChoicesDeselectAnimation()
    {
        rectTransform.sizeDelta = new Vector2(816, 102);
        rectTransform.DOSizeDelta(new Vector2(800, 100), 0.5f, false).SetEase(Ease.OutExpo);
    }

    //private void ChoicesClickAnimation()
    //{
    //    rectTransform.sizeDelta = new Vector2(816, 102);
    //    rectTransform.DOSizeDelta(new Vector2(792, 99), 0.2f, false).SetEase(Ease.OutExpo);
    //    canvasGroup.DOFade(0, 0.5f);
    //}
}
