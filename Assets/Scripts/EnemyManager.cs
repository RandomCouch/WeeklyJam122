using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {

    [SerializeField]
    private Transform _spawnPointLeft;

    [SerializeField]
    private Transform _spawnPointRight;

    [SerializeField]
    private GameObject _enemyPrefab;

    public int maxEnemies = 5;

    private List<EnemyScript> _enemies;

	// Use this for initialization
	void Start () {
        _enemies = new List<EnemyScript>();
        StartCoroutine(SpawnEnemy());
    }
	
	// Update is called once per frame
	void Update () {
	}

    private IEnumerator SpawnEnemy()
    {
        while(_enemies.Count < maxEnemies)
        {
            System.Random rand = new System.Random();
            float randomFloat = rand.Next(0, 2);
            Vector3 chosenSpawnPoint = Vector3.zero;
            if (randomFloat == 0)
            {
                chosenSpawnPoint = _spawnPointLeft.position;
            }
            else
            {
                chosenSpawnPoint = _spawnPointRight.position;
            }
            GameObject newEnemy = Instantiate(_enemyPrefab, chosenSpawnPoint, Quaternion.identity);
            EnemyScript enemyScript = newEnemy.GetComponent<EnemyScript>();
            enemyScript.OnDestroyed += () => { _enemies.Remove(enemyScript); };
            _enemies.Add(enemyScript);
            yield return new WaitForSeconds(2f);
        }
    }
}
