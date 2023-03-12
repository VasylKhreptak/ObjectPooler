using Plugins.ObjectPooler;
using UnityEngine;
using UnityEngine.Profiling;

public class ObjectPoolerSpawn : MonoBehaviour
{
    [SerializeField] private ObjectPooler _objectPooler;
    [SerializeField] private Pool _pool = Pool.TestPool;

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    [ContextMenu("Spawn")]
    private void Spawn()
    {
        Profiler.BeginSample("Spawn");
        
        _objectPooler.Spawn(_pool);
        
        Profiler.EndSample();
    }
}
