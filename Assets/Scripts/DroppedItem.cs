using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public Item item;
    public int amount;
    private Vector2 velocity;
    private float lifetime = 2f;
    private float timer = 0f;
    private SpriteRenderer sr;
    private float minY;
    private bool isGrounded;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        if (item != null && item.icon != null)
        {
            sr.sprite = item.icon;
            sr.enabled = true;
        }
        else
        {
            sr.enabled = false;
        }

        if (amount <= 0)
        {
            amount = 1;
        }

        velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f));
        minY = transform.position.y - 1f;
        isGrounded = false;
    }

    void Update()
    {
        if (!isGrounded)
        {
            transform.Translate(velocity * Time.deltaTime);
            velocity.y -= 5f * Time.deltaTime;
            velocity.x *= 0.95f;

            if (transform.position.y <= minY)
            {
                transform.position = new Vector3(transform.position.x, minY, transform.position.z);
                velocity.y = 0;
                velocity.x = 0;
                isGrounded = true;
            }
        }

        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            if (item != null && Inventory.instance != null)
            {
                if (Inventory.instance.AddItem(item, amount))
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject); // Destroy to prevent lingering
            }
        }
    }
}