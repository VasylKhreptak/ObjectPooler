using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Dictionary<Pool, Queue<GameObject>> _pools;

        #region MonoBehaviour

        private void OnValidate()
        {
            _transform ??= GetComponent<Transform>();

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
            _pools = new Dictionary<Pool, Queue<GameObject>>();

            foreach (var objectPool in _poolsToCreate)
            {
                _pools.Add(objectPool.pool, CreatePool(objectPool));
            }
        }

        private Queue<GameObject> CreatePool(ObjectPool pool)
        {
            var objectPool = new Queue<GameObject>(pool.initialSize);

            Transform poolParent = CreatePoolParent(pool.pool);

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject poolObject = InstantiateObject(pool.prefab);

                poolObject.SetActive(false);

                poolObject.transform.SetParent(poolParent);

                objectPool.Enqueue(poolObject);
            }

            return objectPool;
        }

        protected virtual GameObject InstantiateObject(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private Transform CreatePoolParent(Pool pool)
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

            GameObject poolObject = GetAppropriatePoolObject(pool);

            if (poolObject.activeSelf) // тут ще хз
            {
                poolObject.SetActive(false);
            }

            poolObject.transform.position = position;
            poolObject.transform.rotation = rotation;

            poolObject.SetActive(true);

            return poolObject;
        }

        private GameObject GetAppropriatePoolObject(Pool pool)
        {
            if (TryGetInactive(pool, out GameObject poolObject))
            {
                return poolObject;
            }
            if (IsExpandable(pool))
            {
                return AddPoolObject(pool);
            }

            GameObject poolObj = _pools[pool].Dequeue();
            _pools[pool].Enqueue(poolObj);

            return poolObj;
        }

        private bool TryGetInactive(Pool pool, out GameObject poolObject)
        {
            foreach (var poolItem in _pools[pool])
            {
                if (poolItem.activeSelf == false)
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

        private GameObject AddPoolObject(Pool pool)
        {
            GameObject newPoolObject = InstantiateObject(GetPrefab(pool));

            newPoolObject.SetActive(false);

            newPoolObject.transform.SetParent(GetPoolFolder(pool));

            _pools[pool].Enqueue(newPoolObject);

            return newPoolObject;
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
                poolItem.SetActive(false);
            }
        }

        private Transform GetPoolFolder(Pool pool)
        {
            if (_pools[pool].Count == 0)
            {
                throw new Exception("Pool '" + pool + "' is empty.");
            }

            GameObject poolObject = _pools[pool].First();

            return poolObject.transform.parent;
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
                UnityEngine.Debug.LogError("The size of pool'" + pool.pool + "' must be greater than 0.");
            }
        }

        private void ValidateMaxExpandSize(ObjectPool pool)
        {
            if (pool.maxExpandSize < 1)
            {
                pool.maxExpandSize = 1;
                UnityEngine.Debug.LogError("The max size of pool'" + pool.pool + "' must be greater than 0.");
            }
        }

        private void ValidatePrefab(ObjectPool pool)
        {
            if (pool.prefab == null)
            {
                UnityEngine.Debug.LogError("The prefab of pool'" + pool.pool + "' is null.");
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
