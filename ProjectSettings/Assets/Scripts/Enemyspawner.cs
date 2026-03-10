using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float checkInterval = 2f;
    [SerializeField] private float initialDelay = 1f;

    [Header("Distancia del player")]
    [SerializeField] private float minSpawnDistance = 6f;
    [SerializeField] private float maxSpawnDistance = 12f;
    [SerializeField] private float groundCheckDistance = 5f;

    [Header("Despawn")]
    [SerializeField] private float despawnDistance = 20f; // distancia máxima antes de despawnear

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    private Transform player;
    private Camera mainCam;

    // -------------------------------------------------------
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCam = Camera.main;

        if (player == null)
        {
            Debug.LogWarning("EnemySpawner: No encontró al Player");
            return;
        }

        StartCoroutine(SpawnLoop());
        StartCoroutine(DespawnLoop());
    }

    // -------------------------------------------------------
    void Update()
    {
        if (player != null)
            transform.position = player.position;
    }

    // -------------------------------------------------------
    // SPAWN LOOP
    // -------------------------------------------------------
    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            int currentCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            if (currentCount < maxEnemies)
                TrySpawn();

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // -------------------------------------------------------
    // DESPAWN LOOP — revisa enemies fuera de cámara
    // -------------------------------------------------------
    IEnumerator DespawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;

                // Verificar si está fuera del rango del player
                float dist = Vector2.Distance(enemy.transform.position, player.position);
                if (dist > despawnDistance)
                {
                    Destroy(enemy);
                    continue;
                }

                // Verificar si está fuera de la cámara
                if (!IsVisibleFromCamera(enemy.transform.position))
                {
                    // Solo despawnear si está lejos del player también
                    if (dist > minSpawnDistance)
                        Destroy(enemy);
                }
            }
        }
    }

    // -------------------------------------------------------
    bool IsVisibleFromCamera(Vector3 worldPos)
    {
        if (mainCam == null) return true;
        Vector3 viewportPos = mainCam.WorldToViewportPoint(worldPos);
        return viewportPos.x >= -0.1f && viewportPos.x <= 1.1f &&
               viewportPos.y >= -0.1f && viewportPos.y <= 1.1f &&
               viewportPos.z > 0f;
    }

    // -------------------------------------------------------
    void TrySpawn()
    {
        if (player == null) return;

        int currentCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentCount >= maxEnemies) return;

        for (int i = 0; i < 15; i++)
        {
            float side = Random.value > 0.5f ? 1f : -1f;
            float dist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 candidate = (Vector2)player.position + Vector2.right * side * dist;

            RaycastHit2D hit = Physics2D.Raycast(candidate, Vector2.down, groundCheckDistance, groundLayer);
            if (hit.collider == null) continue;

            Vector2 spawnPos = hit.point + Vector2.up * 1f;

            // No spawnear dentro de la cámara
            if (IsVisibleFromCamera(spawnPos)) continue;

            Collider2D overlap = Physics2D.OverlapCircle(spawnPos, 0.5f, groundLayer);
            if (overlap != null) continue;

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            return;
        }
    }

    // -------------------------------------------------------
    public void OnEnemyDied() { }

    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, despawnDistance);
    }
}