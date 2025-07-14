using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image loadingBarOutline;
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TextMeshProUGUI loadingText;

    private float fillDuration = 5f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    void Start()
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0;
        }
        if (loadingText != null)
        {
            loadingText.text = "Đang tải...";
        }
    }

    public void Show(string sceneName)
    {
        gameObject.SetActive(true); StartCoroutine(FakeLoading(sceneName));
    }

    private IEnumerator FakeLoading(string sceneName)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fillDuration;
            if (loadingBarFill != null)
            {
                loadingBarFill.fillAmount = progress;
            }
            if (loadingText != null)
            {
                loadingText.text = $"Đang tải... {(progress * 100):0}%";
            }
            yield return null;
        }

        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 1f;
        }
        if (loadingText != null)
        {
            loadingText.text = "Đang tải... 100%";
        }

        // Tải scene với additive mode để giữ EventSystem
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!operation.isDone)
        {
            yield return null;
        }

        // Ẩn UI sau khi tải xong
        gameObject.SetActive(false);
    }
}