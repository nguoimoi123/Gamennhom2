using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Canvas bookCanvas;
    [SerializeField] private Canvas playerStatusCanvas;
    [SerializeField] private Canvas weaponSwitchCanvas;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // Hủy các canvas trùng lặp
            if (bookCanvas != null) Destroy(bookCanvas.gameObject);
            if (playerStatusCanvas != null) Destroy(playerStatusCanvas.gameObject);
            if (weaponSwitchCanvas != null) Destroy(weaponSwitchCanvas.gameObject);
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Đảm bảo các canvas được giữ qua các scene
        if (bookCanvas != null) DontDestroyOnLoad(bookCanvas.gameObject);
        if (playerStatusCanvas != null) DontDestroyOnLoad(playerStatusCanvas.gameObject);
        if (weaponSwitchCanvas != null) DontDestroyOnLoad(weaponSwitchCanvas.gameObject);
    }
}