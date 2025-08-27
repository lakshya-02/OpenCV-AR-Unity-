using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    [SerializeField] private GameObject SpawnPrefab;
    [SerializeField] private float SpawnRate = 0.1f;
    [SerializeField] private int MaxParticles = 3;
    [SerializeField] private Vector2 SizeRange;
    private GameObject[] pool;

    void Start()
    {
        initializePool();
        spawn();
    }

    private void initializePool()
    {
        pool = new GameObject[MaxParticles];
        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = Instantiate(SpawnPrefab);
            particle.SetActive(false);
            pool[i] = particle;
        }
    }

    private void spawn()
    {
        foreach (var particle in pool)
        {
            if (!particle.activeSelf)
            {
                particle.transform.position = transform.TransformPoint(Random.insideUnitSphere * 0.5f);
                particle.transform.localScale = Random.Range(SizeRange.x, SizeRange.y) * Vector3.one;
                particle.SetActive(true);
                break;
            }
        }
        Invoke("spawn", SpawnRate);
    }

}
