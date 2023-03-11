using Plugins.ObjectPooler.Events;
using Plugins.ObjectPooler.Linker;
using UnityEngine;

namespace Plugins.ObjectPooler
{
    public class PooledObject : MonoBehaviour
    {
        public PositionLinker linker;
        public Pool pool;
        public OnPooledObjectEnabled enableEvent;
        public OnPooledObjectDisabled disableEvent;
    }
}
