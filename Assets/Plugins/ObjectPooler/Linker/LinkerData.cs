using UnityEngine;

namespace Plugins.ObjectPooler.Linker
{
    [System.Serializable]
    public class LinkerData
    {
        public Transform linkTo;
        public Vector3 offset;
        public Vector3Int axes = Vector3Int.one;
    }
}
