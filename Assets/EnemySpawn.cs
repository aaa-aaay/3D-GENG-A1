using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{

    [SerializeField] private GameObject enemy;
    private int spawnRangeXZ = 7;
    private int spawnRangeY = 3;
    int randomNo;

    public List<GameObject> EnemyObjects = new List<GameObject>();

    private void Update()
    {
        int count = 0;
        EnemyObjects.RemoveAll(item => item == null);
        foreach (GameObject enemy in EnemyObjects) { 
            count++;
        }
        if(count != 10)
        CreateEnemy();
    }

    private void CreateEnemy()
    {

        float randomX = Random.Range(-spawnRangeXZ, spawnRangeXZ);
        float randomY = Random.Range(-spawnRangeY, spawnRangeY);
        float randomZ = Random.Range(-spawnRangeXZ, spawnRangeXZ);

        Vector3 spawnPos = new Vector3(transform.position.x + randomX, transform.position.y + randomY, transform.position.z + randomZ);

        GameObject enemySpawned = Instantiate(enemy, spawnPos, Quaternion.identity);
        EnemyObjects.Add(enemySpawned);
    } 
}
