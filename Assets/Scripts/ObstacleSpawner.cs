using UnityEngine;
using System.Collections.Generic;

// Attach to the ObstacleSpawner empty GameObject.
// Drag obstacle prefabs into the Inspector arrays.
public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public LaneManager laneManager;

    [Header("Obstacle Prefabs")]
    public GameObject[] highObstacles;  // player must DUCK
    public GameObject[] lowObstacles;   // player must JUMP

    [Header("Spawn Settings")]
    public float spawnDistance = 30f;
    public float despawnDistance = 10f;
    public float spawnInterval = 2f;
    public float minInterval = 0.5f;

    private float spawnTimer = 0f;
    private List<GameObject> active = new List<GameObject>();

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnObstacle();
            spawnInterval = Mathf.Max(spawnInterval - 0.01f, minInterval);
            spawnTimer = spawnInterval;
        }

        // Move obstacles backward to simulate forward motion
        PlayerController pc = player?.GetComponent<PlayerController>();
        float speed = pc != null ? pc.GetCurrentSpeed() : GameManager.Instance.currentSpeed;
        
        foreach (var obs in active)
        {
            if (obs != null)
                obs.transform.Translate(Vector3.back * speed * Time.deltaTime);
        }

        // Move ground backward
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
            ground.transform.Translate(Vector3.back * speed * Time.deltaTime);

        Cleanup();
    }

    void SpawnObstacle()
    {
        if (player == null || laneManager == null) return;
        
        int lane = Random.Range(0, 3);
        float x = laneManager.GetLaneX(lane);
        bool high = Random.value > 0.5f;

        GameObject[] pool = high ? highObstacles : lowObstacles;
        if (pool == null || pool.Length == 0)
        {
            // Create obstacle dynamically if no prefab assigned
            CreateObstacleOnSpot(x, high);
            return;
        }

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        if (prefab == null)
        {
            CreateObstacleOnSpot(x, high);
            return;
        }

        Vector3 pos = new Vector3(x, 0f, player.position.z + spawnDistance);
        active.Add(Instantiate(prefab, pos, Quaternion.identity));
    }

    void CreateObstacleOnSpot(float x, bool high)
    {
        GameObject obs = GameObject.CreatePrimitive(high ? PrimitiveType.Cube : PrimitiveType.Cube);
        obs.name = high ? "HighObstacle" : "LowObstacle";
        obs.transform.localScale = high ? new Vector3(2f, 2f, 1f) : new Vector3(2f, 0.5f, 1f);
        obs.transform.position = new Vector3(x, high ? 2f : 0.25f, player.position.z + spawnDistance);
        
        // Configure for collision
        DestroyImmediate(obs.GetComponent<MeshCollider>());
        obs.AddComponent<BoxCollider>();
        Rigidbody rb = obs.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        
        obs.tag = "Obstacle";
        active.Add(obs);
    }

    void Cleanup()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] == null) { active.RemoveAt(i); continue; }
            if (active[i].transform.position.z < player.position.z - despawnDistance)
            {
                Destroy(active[i]);
                active.RemoveAt(i);
            }
        }
    }
}