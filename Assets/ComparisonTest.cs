using System.Diagnostics;
using Plugins.ObjectPooler;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ComparisonTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private Pool _pool;
    [SerializeField] private int _sizeToSpawn;

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    private void Start()
    {
        StartTesting();
    }

    [ContextMenu("Test")]
    private void StartTesting()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Stop();
        stopwatch.Reset();

        stopwatch.Start();
        for (int i = 0; i < _sizeToSpawn; i++)
        {
            _objectPooler.Spawn(_pool);
        }
        stopwatch.Stop();
        Debug.Log("Spawned from pool: " + stopwatch.Elapsed.Ticks);
        
        stopwatch.Reset();

        stopwatch.Start();
        GameObject prefab = _objectPooler.GetPrefab(_pool);
        for (int i = 0; i < _sizeToSpawn; i++)
        {
            Instantiate(prefab);
        }
        stopwatch.Stop();
        Debug.Log("Instantiate time: " + stopwatch.Elapsed.Ticks);
    }
}
