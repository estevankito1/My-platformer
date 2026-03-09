using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // -------------------------------------------------------
    // CONFIGURACIÓN
    // -------------------------------------------------------
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 4;
    [SerializeField] private float spawnInterval = 4f;

    [Header("Distancia del player")]
    [SerializeField] private float minSpawnDistance = 5f;
    [SerializeField] private float maxSpawnDistance = 10f;
    [SerializeField] private float groundCheckDistance = 3f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Transform player;

    // Lista de enemigos vivos — más confiable que un contador
    private List<GameObject> activeEnemies = new List<GameObject>();

    // -------------------------------------------------------
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("EnemySpawner: No encontró al Player");

        StartCoroutine(SpawnLoop());
    }

    // -------------------------------------------------------
    void Update()
    {
        // Limpiar enemigos destruidos de la lista automáticamente
        activeEnemies.RemoveAll(e => e == null);

        if (showDebugInfo)
            Debug.Log($"Enemigos activos: {activeEnemies.Count} / {maxEnemies}");
    }

    // -------------------------------------------------------
    IEnumerator SpawnLoop()
    {
        // Esperar un momento antes del primer spawn
        yield return new WaitForSeconds(2f);

        while (true)
        {
            // Limpiar nulos antes de contar
            activeEnemies.RemoveAll(e => e == null);

            if (activeEnemies.Count < maxEnemies && player != null)
                TrySpawn();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // -------------------------------------------------------
    void TrySpawn()
    {
        for (int i = 0; i < 10; i++)
        {
            float side = Random.value > 0.5f ? 1f : -1f;
            float dist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 candidate = (Vector2)player.position + Vector2.right * side * dist;

            RaycastHit2D hit = Physics2D.Raycast(candidate, Vector2.down, groundCheckDistance, groundLayer);
            if (hit.collider != null)
            {
                // Spawnear encima del suelo con suficiente espacio
                Vector2 spawnPos = hit.point + Vector2.up * 1f;

                // Verificar que no haya nada en esa posición (evitar spawn dentro de paredes)
                Collider2D overlap = Physics2D.OverlapCircle(spawnPos, 0.4f, groundLayer);
                if (overlap != null) continue;

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                // Registrar en la lista
                activeEnemies.Add(enemy);

                // Avisar al enemy quién es su spawner
                enemy.GetComponent<Enemy>()?.SetSpawner(this);
                return;
            }
        }

        Debug.LogWarning("EnemySpawner: No encontró posición válida para spawnear");
    }

    // -------------------------------------------------------
    // Este método sigue existiendo pero ya no es el único control
    public void OnEnemyDied()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    // -------------------------------------------------------
    // GIZMOS
    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
    }
}