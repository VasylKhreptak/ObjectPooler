namespace Plugins.ObjectPooler.Events.Core
{
    public class PooledObjectEvent : UnityEngine.MonoBehaviour
    {
        public event System.Action<PooledObject> onMonoCall;

        protected void Invoke(PooledObject pooledObject) => onMonoCall?.Invoke(pooledObject);
    }
}
