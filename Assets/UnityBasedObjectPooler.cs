using UnityEngine;
using UnityEngine.Pool;

public class UnityBasedObjectPooler : MonoBehaviour
{
    private ObjectPool<GameObject> _pool;

    #region MonoBehaviour

    private void Awake()
    {
        _pool = new ObjectPool<GameObject>(CreatePoolObject, defaultCapacity: 5, maxSize: 5000);
        
        _pool.Get();
    }

    #endregion

    private GameObject CreatePoolObject()
    {
        return new GameObject("Pool Object");
    }
}
