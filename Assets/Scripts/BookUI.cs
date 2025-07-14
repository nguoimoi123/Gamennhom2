using UnityEngine;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    [SerializeField] private Canvas bookCanvas;
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private GameObject[] pagePairs;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    private int currentPageIndex = 0;
    private bool isOpen = false;
    private bool isOpening = false;
    private bool isClosing = false;
    private bool isFlipping = false;

    void Start()
    {
        bool hasError = false;
        if (bookCanvas == null) { Debug.LogError("BookUI: 'bookCanvas' is not assigned!", this); hasError = true; }
        if (bookAnimator == null) { Debug.LogError("BookUI: 'bookAnimator' is not assigned!", this); hasError = true; }
        if (pagePairs == null || pagePairs.Length == 0) { Debug.LogError("BookUI: 'pagePairs' is not assigned or empty!", this); hasError = true; }
        if (leftArrowButton == null) { Debug.LogError("BookUI: 'leftArrowButton' is not assigned!", this); hasError = true; }
        if (rightArrowButton == null) { Debug.LogError("BookUI: 'rightArrowButton' is not assigned!", this); hasError = true; }

        if (hasError)
        {
            Debug.LogError("BookUI: Missing required components! Check Inspector.", this);
            return;
        }

        bookCanvas.enabled = false;
        for (int i = 0; i < pagePairs.Length; i++)
        {
            pagePairs[i].SetActive(false);
        }

        leftArrowButton.onClick.AddListener(FlipPageLeft);
        rightArrowButton.onClick.AddListener(FlipPageRight);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isOpening && !isClosing && !isFlipping)
        {
            if (!isOpen)
            {
                isOpening = true;
                bookCanvas.enabled = true;
                bookAnimator.Play("OpenBook", 0, 0);
                Debug.Log("Opening book");
            }
            else
            {
                isClosing = true;
                for (int i = 0; i < pagePairs.Length; i++)
                {
                    pagePairs[i].SetActive(false);
                }
                bookAnimator.Play("CloseBook", 0, 0);
                Debug.Log("Closing book");
            }
        }
    }

    public void OnOpenComplete()
    {
        if (isOpening)
        {
            isOpening = false;
            isOpen = true;
            UpdatePageVisibility();
            Debug.Log("Book opened");
        }
    }

    public void OnCloseComplete()
    {
        if (isClosing)
        {
            isClosing = false;
            isOpen = false;
            bookCanvas.enabled = false;
            Debug.Log("Book closed");
        }
    }

    public void OnPageFlipComplete()
    {
        if (isFlipping)
        {
            isFlipping = false;
            UpdatePageVisibility();
            Debug.Log($"Flipped to page {currentPageIndex + 1}");
        }
    }

    private void FlipPageLeft()
    {
        if (isFlipping || currentPageIndex <= 0) return;
        isFlipping = true;
        int previousPageIndex = currentPageIndex;
        currentPageIndex--;
        CanvasGroup previousCanvasGroup = pagePairs[previousPageIndex].GetComponent<CanvasGroup>();
        if (previousCanvasGroup != null)
        {
            previousCanvasGroup.alpha = 0f;
        }
        else
        {
            pagePairs[previousPageIndex].SetActive(false);
        }
        bookAnimator.Play("FlipLeft", 0, 0);
        Debug.Log($"Starting FlipLeft to page {currentPageIndex + 1}");
    }

    private void FlipPageRight()
    {
        if (isFlipping || currentPageIndex >= pagePairs.Length - 1) return;
        isFlipping = true;
        int previousPageIndex = currentPageIndex;
        currentPageIndex++;
        CanvasGroup previousCanvasGroup = pagePairs[previousPageIndex].GetComponent<CanvasGroup>();
        if (previousCanvasGroup != null)
        {
            previousCanvasGroup.alpha = 0f;
        }
        else
        {
            pagePairs[previousPageIndex].SetActive(false);
        }
        bookAnimator.Play("FlipRight", 0, 0);
        Debug.Log($"Starting FlipRight to page {currentPageIndex + 1}");
    }

    private void UpdatePageVisibility()
    {
        for (int i = 0; i < pagePairs.Length; i++)
        {
            CanvasGroup canvasGroup = pagePairs[i].GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = (i == currentPageIndex) ? 1f : 0f;
            }
            else
            {
                pagePairs[i].SetActive(i == currentPageIndex);
            }
        }
        leftArrowButton.interactable = currentPageIndex > 0;
        rightArrowButton.interactable = currentPageIndex < pagePairs.Length - 1;
    }
}