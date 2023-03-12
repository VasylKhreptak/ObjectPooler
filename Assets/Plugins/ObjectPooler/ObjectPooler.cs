using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.ObjectPooler.Linker;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Plugins.ObjectPooler
{
    public class ObjectPooler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _transform;

        [Header("Preferences")]
        [Tooltip("Allocates memory for the maximum size of the pool(MaxExpandSize)."
            + " This will reduce the amount of memory allocations, but will increase the memory usage.")]
        [SerializeField] private bool _allocateMaxMemorySize;

        [Header("Pools")]
        [SerializeField] private PoolPreference[] _poolsPreferences;

        private Dictionary<Pool, Queue<PooledObject>> _pools;
        private Dictionary<Pool, HashSet<PooledObject>> _activePools;
        private Dictionary<Pool, HashSet<PooledObject>> _inactivePools;

        private int GetCollectionCapacity(PoolPreference preference)
        {
            return _allocateMaxMemorySize ? preference.maxExpandSize : preference.initialSize;
        }

        #region MonoBehaviour

        private void OnValidate()
        {
            _transform ??= GetComponent<Transform>();

            if (_poolsPreferences == null) return;

            ValidateInputData();
        }

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        #endregion

        #region Fill

        private void Init()
        {
            _pools = new Dictionary<Pool, Queue<PooledObject>>();
            _activePools = new Dictionary<Pool, HashSet<PooledObject>>();
            _inactivePools = new Dictionary<Pool, HashSet<PooledObject>>();

            foreach (var poolPreferences in _poolsPreferences)
            {
                _pools.Add(poolPreferences.pool, CreatePool(poolPreferences));
                _activePools.Add(poolPreferences.pool, new HashSet<PooledObject>(GetCollectionCapacity(poolPreferences)));
                _inactivePools.Add(poolPreferences.pool, _pools[poolPreferences.pool].ToHashSet());
            }
        }

        private Queue<PooledObject> CreatePool(PoolPreference poolPreference)
        {
            var objectPool = new Queue<PooledObject>(GetCollectionCapacity(poolPreference));

            Transform poolFolder = CreatePoolFolder(poolPreference.pool);

            for (int i = 0; i < poolPreference.initialSize; i++)
            {
                PooledObject newPoolObject = CreateNewPoolObject(poolPreference.pool, poolFolder);
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

        #region ActiveInactivePoolsManagement

        private void AddListener(PooledObject pooledObject)
        {
            pooledObject.onEnable += OnObjectEnabled;
            pooledObject.onDisable += OnObjectDisabled;
        }

        private void RemoveListener(PooledObject pooledObject)
        {
            pooledObject.onEnable -= OnObjectEnabled;
            pooledObject.onDisable -= OnObjectDisabled;
        }

        private void RemoveListeners()
        {
            foreach (var pool in _pools)
            {
                foreach (var pooledObject in _pools[pool.Key])
                {
                    RemoveListener(pooledObject);
                }
            }
        }

        private void AddListeners()
        {
            foreach (var pool in _pools)
            {
                foreach (var pooledObject in _pools[pool.Key])
                {
                    AddListener(pooledObject);
                }
            }
        }

        private void RemoveFromActivePool(PooledObject pooledObject) => _activePools[pooledObject.pool].Remove(pooledObject);

        private void AddToActivePool(PooledObject pooledObject) => _activePools[pooledObject.pool].Add(pooledObject);

        private void RemoveFromInactivePool(PooledObject pooledObject) => _inactivePools[pooledObject.pool].Remove(pooledObject);

        private void AddToInactivePool(PooledObject pooledObject) => _inactivePools[pooledObject.pool].Add(pooledObject);

        private void OnObjectEnabled(PooledObject pooledObject)
        {
            AddToActivePool(pooledObject);
            RemoveFromInactivePool(pooledObject);
        }

        private void OnObjectDisabled(PooledObject pooledObject)
        {
            RemoveFromActivePool(pooledObject);
            AddToInactivePool(pooledObject);
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

            Transform poolObjectTransform = poolObject.transform;
            poolObjectTransform.position = position;
            poolObjectTransform.rotation = rotation;

            GameObject pooledGameObject = poolObject.gameObject;
            pooledGameObject.SetActive(true);

            return pooledGameObject;
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
            if (_inactivePools[pool].Count > 0)
            {
                poolObject = _inactivePools[pool].First();
                return true;
            }

            poolObject = null;
            return false;
        }

        public GameObject GetPrefab(Pool pool)
        {
            foreach (var objectPool in _poolsPreferences)
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
            foreach (var poolPreferences in _poolsPreferences)
            {
                if (poolPreferences.pool == pool)
                {
                    return poolPreferences.autoExpand && _pools[pool].Count < poolPreferences.maxExpandSize;
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
            pooledObject.pool = pool;
            pooledObject.linker = newPoolObject.AddComponent<PositionLinker>();

            AddListener(pooledObject);

            return pooledObject;
        }

        #endregion

        #region Utils

        public void DisableAllObjects()
        {
            foreach (var pool in _activePools)
            {
                DisablePool(pool.Key);
            }
        }

        public void DisablePool(Pool pool)
        {
            foreach (var poolItem in _activePools[pool].ToArray())
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

            pooledObject = null;
            return false;
        }

        #endregion

        #region DataValidation

        private void ValidateInputData()
        {
            foreach (var pool in _poolsPreferences)
            {
                ValidateInitialSize(pool);

                ValidateMaxExpandSize(pool);

                ValidatePrefab(pool);
            }
        }

        private void ValidateInitialSize(PoolPreference poolPreference)
        {
            if (poolPreference.initialSize < 1)
            {
                poolPreference.initialSize = 1;
                Debug.LogError("The size of pool'" + poolPreference.pool + "' must be greater than 0.");
            }
        }

        private void ValidateMaxExpandSize(PoolPreference poolPreference)
        {
            if (poolPreference.maxExpandSize < 1)
            {
                poolPreference.maxExpandSize = 1;
                Debug.LogError("The max size of pool'" + poolPreference.pool + "' must be greater than 0.");
            }
        }

        private void ValidatePrefab(PoolPreference poolPreference)
        {
            if (poolPreference.prefab == null)
            {
                Debug.LogError("The prefab of pool'" + poolPreference.pool + "' is null.");
            }
        }

        #endregion

        [Serializable]
        private class PoolPreference
        {
            public Pool pool;
            public GameObject prefab;
            public int initialSize = 5;
            public bool autoExpand;
            public int maxExpandSize = 20;
        }
    }
}
