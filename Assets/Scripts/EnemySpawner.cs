using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float respawnDelay = 10f;
    private EnemyBase spawnedEnemy;

    private void Start()
    {
        SpawnEnemy();
    }

    public void NotifyEnemyDeath(EnemyBase enemy)
    {
        StartCoroutine(RespawnEnemyAfterDelay());
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            spawnedEnemy = enemyObj.GetComponent<EnemyBase>();
            if (spawnedEnemy != null)
            {
                spawnedEnemy.SetSpawner(this);
            }
        }
    }

    private IEnumerator RespawnEnemyAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (spawnedEnemy != null)
        {
            spawnedEnemy.Respawn();
        }
        else
        {
            SpawnEnemy();
        }
    }
}