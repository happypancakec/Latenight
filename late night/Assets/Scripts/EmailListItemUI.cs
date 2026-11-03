using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private Button button;

    public void Setup(
        EmailUIController.EmailData email,
        Action<EmailUIController.EmailData> onSelected
    )
    {
        senderText.text = email.sender;
        subjectText.text = email.subject;
        previewText.text = CreatePreview(email.message);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            () => onSelected(email)
        );
    }

    private string CreatePreview(string message)
    {
        const int previewLength = 45;

        if (string.IsNullOrWhiteSpace(message))
        {
            return "";
        }

        string singleLine = message.Replace("\n", " ");

        if (singleLine.Length <= previewLength)
        {
            return singleLine;
        }

        return singleLine.Substring(0, previewLength) + "...";
    }
}