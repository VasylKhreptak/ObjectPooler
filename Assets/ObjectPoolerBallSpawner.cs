using System;
using System.Collections;
using Plugins.ObjectPooler;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPoolerBallSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private Pool _pool;
    [SerializeField] private float _delay = 0.1f;

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    private IEnumerator Start()
    {
        while (true)
        {
            for (int i = 0; i < 100; i++)
            {
                GameObject pooledObject = _objectPooler.Spawn(_pool);

                if (pooledObject.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.AddForce(Random.insideUnitSphere * 50f, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(_delay);
        }
    }
}
