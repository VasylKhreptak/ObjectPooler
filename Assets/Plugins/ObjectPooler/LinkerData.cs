using UnityEngine;

namespace Plugins.ObjectPooler
{
    [System.Serializable]
    public class LinkerData
    {
        public Transform linkTo;
        public Vector3 offset;
        public Vector3Int axes = UnityEngine.Vector3Int.one;
    }
}
