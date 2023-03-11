using System;
using System.Diagnostics;
using Plugins.ObjectPooler;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ObjectPoolerStressTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private int _size = 5000;
    [SerializeField] private Pool _pool = Pool.TestPool2;
    [SerializeField] private int _testCount = 5;

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    [ContextMenu("Start Test")]
    private void StartTest()
    {
        for (int i = 0; i < _testCount; i++)
        {
            DoTest();
        }
    }

    private void DoTest()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < _size; i++)
        {
            _objectPooler.Spawn(_pool);
        }

        _objectPooler.DisablePool(_pool);

        stopwatch.Stop();
        Debug.Log("Stress Test Time: " + stopwatch.Elapsed.TotalSeconds);
    }
}
