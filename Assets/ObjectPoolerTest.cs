using System.Collections;
using System.Diagnostics;
using Plugins.ObjectPooler;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPoolerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private Pool _pool;
    [SerializeField] private int _sizeToSpawn;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _startDelay = 1f;
    [SerializeField] private bool _useDelay;
    [SerializeField] private float _delay = 0;

    #region MonoBehaviour

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    [ContextMenu("Do Test")]
    private void DoTest()
    {
        StartCoroutine(TestingRoutine());
    }

    #endregion

    private IEnumerator TestingRoutine()
    {
        yield return new WaitForSeconds(_startDelay);

        StartCoroutine(SpawnObjects());

        //DisableObjects();
    }

    private IEnumerator SpawnObjects()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < _sizeToSpawn; i++)
        {
            Vector3 position = Random.insideUnitSphere * _maxDistance;
            _objectPooler.Spawn(_pool, position);

            if (_useDelay)
            {
                yield return new WaitForSeconds(_delay);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log("Spawn Time: " + stopwatch.Elapsed.TotalSeconds);
    }

    private void DisableObjects()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        _objectPooler.DisableAllObjects();

        stopwatch.Stop();
        UnityEngine.Debug.Log("Disable Time: " + stopwatch.Elapsed.TotalSeconds);
    }
}
