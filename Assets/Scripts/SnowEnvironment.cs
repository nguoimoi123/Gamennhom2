using UnityEngine;

public class SnowEnvironment : MonoBehaviour
{
    [Header("Debuff Settings")]
    [SerializeField] private float coldHealthDecreaseRate = 0.5f; // Tốc độ giảm máu do lạnh
    [SerializeField] private float hungerDebuffMultiplier = 2f; // Nhân tốc độ giảm đói
    [SerializeField] private float thirstDebuffMultiplier = 2f; // Nhân tốc độ giảm khát

    [Header("Blizzard Settings")]
    [SerializeField] private float blizzardInterval = 30f; // Thời gian giữa các cơn bão
    [SerializeField] private float blizzardDuration = 10f; // Thời gian bão kéo dài
    [SerializeField] private float speedReductionMultiplier = 0.5f; // Giảm tốc độ khi có bão
    [SerializeField] private ParticleSystem blizzardEffect; // Hiệu ứng bão tuyết

    private PlayerStatus playerStatus;
    private float originalHungerDecreaseRate;
    private float originalThirstDecreaseRate;
    private float originalMoveSpeed;
    private float blizzardTimer;
    private bool isBlizzardActive;

    void Start()
    {
        playerStatus = FindAnyObjectByType<PlayerStatus>();
        if (playerStatus != null)
        {
            // Lấy giá trị gốc của các trường private bằng Reflection
            originalHungerDecreaseRate = playerStatus.GetComponent<PlayerStatus>().GetType().GetField("hungerDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(playerStatus) as float? ?? 0f;
            originalThirstDecreaseRate = playerStatus.GetComponent<PlayerStatus>().GetType().GetField("thirstDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(playerStatus) as float? ?? 0f;
            // Giả sử PlayerStatus hoặc script điều khiển người chơi có trường moveSpeed (cần thêm nếu chưa có)
            originalMoveSpeed = 5f; // Giả định tốc độ mặc định, bạn cần điều chỉnh theo script của bạn
        }
        blizzardTimer = blizzardInterval;
        isBlizzardActive = false;
        if (blizzardEffect != null)
        {
            blizzardEffect.Stop();
        }
    }

    void Update()
    {
        if (playerStatus == null || playerStatus.GetComponent<PlayerStatus>().GetType().GetField("isDead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(playerStatus) as bool? == true)
        {
            return;
        }

        ApplyColdDebuff();
        ManageBlizzard();
    }

    private void ApplyColdDebuff()
    {
        if (playerStatus.GetOuterShirt() == null || playerStatus.GetShoes() == null)
        {
            // Giảm máu do lạnh
            playerStatus.TakeDamage(coldHealthDecreaseRate * Time.deltaTime);

            // Tăng tốc độ giảm đói và khát
            playerStatus.GetComponent<PlayerStatus>().GetType().GetField("hungerDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(playerStatus, originalHungerDecreaseRate * hungerDebuffMultiplier);
            playerStatus.GetComponent<PlayerStatus>().GetType().GetField("thirstDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(playerStatus, originalThirstDecreaseRate * thirstDebuffMultiplier);
        }
        else
        {
            // Khôi phục tốc độ giảm đói và khát
            playerStatus.GetComponent<PlayerStatus>().GetType().GetField("hungerDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(playerStatus, originalHungerDecreaseRate);
            playerStatus.GetComponent<PlayerStatus>().GetType().GetField("thirstDecreaseRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(playerStatus, originalThirstDecreaseRate);
        }
    }

    private void ManageBlizzard()
    {
        blizzardTimer -= Time.deltaTime;

        if (blizzardTimer <= 0)
        {
            if (!isBlizzardActive)
            {
                // Bắt đầu bão
                isBlizzardActive = true;
                blizzardTimer = blizzardDuration;
                if (blizzardEffect != null)
                {
                    blizzardEffect.Play();
                }
                ApplyBlizzardDebuff(true);
            }
            else
            {
                // Kết thúc bão
                isBlizzardActive = false;
                blizzardTimer = blizzardInterval;
                if (blizzardEffect != null)
                {
                    blizzardEffect.Stop();
                }
                ApplyBlizzardDebuff(false);
            }
        }
    }

    private void ApplyBlizzardDebuff(bool apply)
    {
        // Giả sử bạn có script PlayerMovement để điều khiển tốc độ di chuyển
        // Cần thêm trường moveSpeed vào script điều khiển người chơi nếu chưa có
        PlayerController playerMovement = playerStatus.GetComponent<PlayerController>();
        if (playerMovement != null)
        {
            if (apply)
            {
                // Giảm tốc độ di chuyển
                float newSpeed = originalMoveSpeed * speedReductionMultiplier;
                playerMovement.GetType().GetField("moveSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(playerMovement, newSpeed);
            }
            else
            {
                // Khôi phục tốc độ di chuyển
                playerMovement.GetType().GetField("moveSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(playerMovement, originalMoveSpeed);
            }
        }
    }
}