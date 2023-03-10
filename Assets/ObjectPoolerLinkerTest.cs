using Plugins.ObjectPooler;
using UnityEngine;

public class ObjectPoolerLinkerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler _objectPooler;

    [Header("Preferences")]
    [SerializeField] private Pool _pool;
    [SerializeField] private LinkerData _linkerData;

    private GameObject _currentObject;

    #region MonoBehaviour

    private void OnValidate()
    {
        _objectPooler ??= FindObjectOfType<ObjectPooler>();
    }

    #endregion

    [ContextMenu("SpawnObject")]
    private void SpawnObject()
    {
        _currentObject = _objectPooler.Spawn(_pool, Vector3.zero);
    }

    [ContextMenu("SpawnObjectAndStartLinking")]
    private void SpawnObjectAndStartLinking()
    {
        _currentObject = _objectPooler.Spawn(_pool, Vector3.zero, _linkerData);
    }

    [ContextMenu("StartLinking")]
    private void StartLinking()
    {
        _objectPooler.StartLinking(_currentObject, _linkerData);
    }

    [ContextMenu("StopLinking")]
    private void StopLinking()
    {
        _objectPooler.StopLinking(_currentObject);
    }
}
