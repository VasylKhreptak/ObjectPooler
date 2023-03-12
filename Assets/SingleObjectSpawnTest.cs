using System;
using System.Diagnostics;
using Plugins.ObjectPooler;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SingleObjectSpawnTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private Pool _pool;

    private GameObject _prefab;

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    private void Start()
    {
        _prefab = _objectPooler.GetPrefab(_pool);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }

    [ContextMenu("Spawn")]
    private void Spawn()
    { 
        Stopwatch stopwatch = new Stopwatch();
        GameObject instance;

        stopwatch.Start();
        instance = _objectPooler.Spawn(_pool);
        instance.SetActive(false);
        stopwatch.Stop();
        Debug.Log("Elapsed ticks(ObjectPooler): " + stopwatch.Elapsed.Ticks);
        stopwatch.Reset();

        stopwatch.Start();
        instance = Instantiate(_prefab);
        Destroy(instance);
        stopwatch.Stop();
        Debug.Log("Elapsed ticks(Instantiate): " + stopwatch.Elapsed.Ticks);
        stopwatch.Reset();
    }
}
