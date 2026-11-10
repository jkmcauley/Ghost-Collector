using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private ObjectPool _ghostPool;
    [SerializeField] private ObjectPool _enemyPool;
    [SerializeField, Min(0f)] private float _spawnRadius = 3f;
    [SerializeField, Min(0.1f)] private float _ghostSpawnInterval = 2f;

    private readonly List<GameObject> _liveEnemies = new();
    private Coroutine _ghostSpawnRoutine;
    private Coroutine _enemySpawnRoutine;

    private void OnEnable()
    {
        _ghostSpawnRoutine = StartCoroutine(SpawnGhostsRoutine());
        _enemySpawnRoutine = StartCoroutine(SpawnEnemiesRoutine());
    }

    private void OnDisable()
    {
        if (_ghostSpawnRoutine != null)
        {
            StopCoroutine(_ghostSpawnRoutine);
            _ghostSpawnRoutine = null;
        }

        if (_enemySpawnRoutine != null)
        {
            StopCoroutine(_enemySpawnRoutine);
            _enemySpawnRoutine = null;
        }
    }

    private IEnumerator SpawnGhostsRoutine()
    {
        while (true)
        {
            if (!PlayHasStarted())
            {
                yield return null;
                continue;
            }

            SpawnGhost();
            yield return new WaitForSeconds(GhostSpawnInterval());
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            if (!PlayHasStarted())
            {
                yield return null;
                continue;
            }

            int waveSize = 1;
            float gap = 0f;
            if (GameManager.Instance != null)
            {
                waveSize = Mathf.Max(1, GameManager.Instance.EnemiesAtOnce);
                gap = Mathf.Max(0f, GameManager.Instance.EnemySpawnGap);
            }

            _liveEnemies.Clear();
            float formation = Random.Range(0f, Mathf.PI * 2f);
            for (int i = 0; i < waveSize; i++)
            {
                if (i > 0 && gap > 0f)
                    yield return new WaitForSeconds(gap);

                if (_player == null)
                    yield break;

                float angle = formation + Mathf.PI * 2f * i / waveSize;
                Vector2 approach = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                bool huntsGhost = i > 0;
                GameObject enemy = SpawnEnemy(approach, huntsGhost, i - 1);
                while (enemy == null)
                {
                    yield return null;
                    if (_player == null)
                        yield break;

                    enemy = SpawnEnemy(approach, huntsGhost, i - 1);
                }

                _liveEnemies.Add(enemy);
            }

            while (!WaveCleared())
                yield return null;
        }
    }

    private static bool PlayHasStarted()
    {
        return GameManager.Instance != null && GameManager.Instance.PlayStarted;
    }

    private float GhostSpawnInterval()
    {
        if (GameManager.Instance == null)
            return _ghostSpawnInterval;

        return Mathf.Max(0.1f, GameManager.Instance.GhostSpawnInterval);
    }

    private bool WaveCleared()
    {
        for (int i = 0; i < _liveEnemies.Count; i++)
        {
            GameObject enemy = _liveEnemies[i];
            if (enemy == null || !enemy.activeInHierarchy)
                continue;

            if (enemy.TryGetComponent(out EnemyController controller) && controller.IsDead)
                continue;

            return false;
        }

        return true;
    }

    public void SpawnGhost()
    {
        if (_ghostPool == null || _player == null)
            return;

        GameObject ghostObject = _ghostPool.TrySpawn(GetRandomPosition());
        if (ghostObject != null && ghostObject.TryGetComponent(out Ghost ghost))
            ghost.Initialize(_player);
    }

    public GameObject SpawnEnemy(Vector2 approach, bool huntsGhost, int ghostSlot)
    {
        if (_enemyPool == null || _player == null)
            return null;

        Vector3 position = _player.position + new Vector3(approach.x, approach.y, 0f) * _spawnRadius;
        GameObject enemyObject = _enemyPool.TrySpawn(position);
        if (enemyObject != null && enemyObject.TryGetComponent(out EnemyAI ai))
            ai.Initialize(_player, huntsGhost, ghostSlot, approach);

        return enemyObject;
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 offset = Random.insideUnitCircle * _spawnRadius;
        return _player.position + new Vector3(offset.x, offset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (_player == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_player.position, _spawnRadius);
    }
}
