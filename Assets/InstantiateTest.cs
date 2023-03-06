using System.Diagnostics;
using UnityEngine;

public class InstantiateTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _size;
    [SerializeField] private float _range = 100f;

    private void Awake()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < _size; i++)
        {
            Vector3 position = Random.insideUnitSphere * _range;

            Instantiate(_prefab, position, Quaternion.identity);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log("Instantiate Time: " + stopwatch.Elapsed.TotalSeconds);
    }
}
