using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportConfirmationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private System.Action onConfirm;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    void Start()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }
    }

    public void Show(string message, System.Action onConfirmAction)
    {
        messageText.text = message;
        onConfirm = onConfirmAction;
        yesButton.gameObject.SetActive(onConfirmAction != null);
        noButton.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    private void OnYesClicked()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }

    private void OnNoClicked()
    {
        gameObject.SetActive(false);
    }
}