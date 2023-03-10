using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins.ObjectPooler
{
    public class ObjectPooler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _transform;

        [Header("Preferences")]
        [SerializeField] private ObjectPool[] _poolsToCreate;

        private Dictionary<Pool, Queue<PooledObject>> _pools;

        #region MonoBehaviour

        private void OnValidate()
        {
            _transform ??= GetComponent<Transform>();

            if (_poolsToCreate == null) return;
            
            ValidateInputData();
        }

        private void Awake()
        {
            Init();
        }

        #endregion

        #region Fill

        private void Init()
        {
            _pools = new Dictionary<Pool, Queue<PooledObject>>();

            foreach (var objectPool in _poolsToCreate)
            {
                _pools.Add(objectPool.pool, CreatePool(objectPool));
            }
        }

        private Queue<PooledObject> CreatePool(ObjectPool pool)
        {
            var objectPool = new Queue<PooledObject>(pool.initialSize);

            Transform poolFolder = CreatePoolFolder(pool.pool);

            for (int i = 0; i < pool.initialSize; i++)
            {
                PooledObject newPoolObject = CreateNewPoolObject(pool.pool, poolFolder);
                objectPool.Enqueue(newPoolObject);
            }

            return objectPool;
        }

        protected virtual GameObject InstantiateObject(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private Transform CreatePoolFolder(Pool pool)
        {
            GameObject poolParentObj = new GameObject(pool.ToString());

            poolParentObj.transform.SetParent(_transform);

            return poolParentObj.transform;
        }

        #endregion

        #region Spawn

        public GameObject Spawn(Pool pool) => Spawn(pool, Vector3.zero, Quaternion.identity);

        public GameObject Spawn(Pool pool, Vector3 position) => Spawn(pool, position, Quaternion.identity);

        public GameObject Spawn(Pool pool, Vector3 position, Quaternion rotation)
        {
            if (_pools.ContainsKey(pool) == false)
            {
                throw new ArgumentException("Pool '" + pool + "' does not exist.");
            }

            PooledObject poolObject = GetAppropriatePoolObject(pool);

            if (poolObject.gameObject.activeSelf)
            {
                poolObject.gameObject.SetActive(false);
            }

            poolObject.transform.position = position;
            poolObject.transform.rotation = rotation;

            poolObject.gameObject.SetActive(true);

            return poolObject.gameObject;
        }

        public GameObject Spawn(Pool pool, Vector3 position, LinkerData data) => Spawn(pool, position, Quaternion.identity, data);

        public GameObject Spawn(Pool pool, LinkerData data) => Spawn(pool, Vector3.zero, Quaternion.identity, data);

        public GameObject Spawn(Pool pool, Vector3 position, Quaternion rotation, LinkerData data)
        {
            GameObject spawnedObject = Spawn(pool, position, rotation);
            StartLinking(spawnedObject, data);

            return spawnedObject;
        }

        public void StartLinking(GameObject gameObject, LinkerData data)
        {
            if (IsObjectPooled(gameObject, out PooledObject pooledObject))
            {
                pooledObject.linker.StartUpdating(data);
            }
        }

        public void StopLinking(GameObject gameObject)
        {
            if (IsObjectPooled(gameObject, out PooledObject pooledObject))
            {
                pooledObject.linker.StopUpdating();
            }
        }

        private PooledObject GetAppropriatePoolObject(Pool pool)
        {
            if (TryGetInactive(pool, out PooledObject poolObject))
            {
                return poolObject;
            }
            if (IsExpandable(pool))
            {
                return AddPoolObject(pool);
            }

            PooledObject poolObj = _pools[pool].Dequeue();
            _pools[pool].Enqueue(poolObj);

            return poolObj;
        }

        private bool TryGetInactive(Pool pool, out PooledObject poolObject)
        {
            foreach (var poolItem in _pools[pool])
            {
                if (poolItem.gameObject.activeSelf == false)
                {
                    poolObject = poolItem;
                    return true;
                }
            }

            poolObject = null;
            return false;
        }

        private GameObject GetPrefab(Pool pool)
        {
            foreach (var objectPool in _poolsToCreate)
            {
                if (objectPool.pool == pool)
                {
                    return objectPool.prefab;
                }
            }

            throw new ArgumentException("Pool '" + pool + "' does not exist.");
        }

        private bool IsExpandable(Pool pool)
        {
            foreach (var objectPool in _poolsToCreate)
            {
                if (objectPool.pool == pool)
                {
                    return objectPool.autoExpand && _pools[pool].Count < objectPool.maxExpandSize;
                }
            }

            throw new ArgumentException("Pool '" + pool + "' does not exist.");
        }

        private PooledObject AddPoolObject(Pool pool) => AddPoolObject(pool, GetPoolFolder(pool));

        private PooledObject AddPoolObject(Pool pool, Transform folder)
        {
            PooledObject newPoolObject = CreateNewPoolObject(pool, folder);

            _pools[pool].Enqueue(newPoolObject);

            return newPoolObject;
        }

        private PooledObject CreateNewPoolObject(Pool pool, Transform folder)
        {
            GameObject newPoolObject = InstantiateObject(GetPrefab(pool));

            newPoolObject.SetActive(false);

            newPoolObject.transform.SetParent(folder);

            PooledObject pooledObject = newPoolObject.AddComponent<PooledObject>();
            pooledObject.linker = newPoolObject.AddComponent<PositionLinker>();

            return pooledObject;
        }

        #endregion

        #region Utils

        public void DisableAllObjects()
        {
            foreach (var pool in _pools)
            {
                DisablePool(pool.Key);
            }
        }

        public void DisablePool(Pool pool)
        {
            foreach (var poolItem in _pools[pool])
            {
                poolItem.gameObject.SetActive(false);
            }
        }

        private Transform GetPoolFolder(Pool pool)
        {
            if (_pools[pool].Count == 0)
            {
                throw new Exception("Pool '" + pool + "' is empty.");
            }

            PooledObject poolObject = _pools[pool].First();

            return poolObject.transform.parent;
        }

        private bool IsObjectPooled(GameObject gameObject, out PooledObject pooledObject)
        {
            if (gameObject.TryGetComponent(out PooledObject targetObject))
            {
                pooledObject = targetObject;
                return true;
            }

            Debug.LogWarning("GameObject '" + gameObject.name + "' does not have PooledObject component.");

            pooledObject = null;
            return false;
        }

        #endregion

        #region DataValidation

        private void ValidateInputData()
        {
            foreach (var pool in _poolsToCreate)
            {
                ValidateInitialSize(pool);

                ValidateMaxExpandSize(pool);

                ValidatePrefab(pool);
            }
        }

        private void ValidateInitialSize(ObjectPool pool)
        {
            if (pool.initialSize < 1)
            {
                pool.initialSize = 1;
                Debug.LogError("The size of pool'" + pool.pool + "' must be greater than 0.");
            }
        }

        private void ValidateMaxExpandSize(ObjectPool pool)
        {
            if (pool.maxExpandSize < 1)
            {
                pool.maxExpandSize = 1;
                Debug.LogError("The max size of pool'" + pool.pool + "' must be greater than 0.");
            }
        }

        private void ValidatePrefab(ObjectPool pool)
        {
            if (pool.prefab == null)
            {
                Debug.LogError("The prefab of pool'" + pool.pool + "' is null.");
            }
        }

        #endregion

        [Serializable]
        private class ObjectPool
        {
            public Pool pool;
            public GameObject prefab;
            public int initialSize = 5;
            public bool autoExpand;
            public int maxExpandSize = 20;
        }
    }
}
