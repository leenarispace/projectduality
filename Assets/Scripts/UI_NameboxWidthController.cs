using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NameboxWidthController : MonoBehaviour
{
    public TextMeshProUGUI charNameText;
    public Image image;
    private void Start()
    {
        float textWidth = charNameText.preferredWidth + 25.0f;
        float imageFillAmount = textWidth / 398.0f;
        image.GetComponent<Image>().fillAmount = imageFillAmount;
    }
}