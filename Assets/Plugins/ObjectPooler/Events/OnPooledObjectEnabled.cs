using Plugins.ObjectPooler.Events.Core;

namespace Plugins.ObjectPooler.Events
{
    public class OnPooledObjectEnabled : PooledObjectEvent
    {
        public PooledObject pooledObject;

        #region MonoBehavior

        private void OnEnable()
        {
            Invoke(pooledObject);
        }

        #endregion
    }
}
