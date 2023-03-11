using Plugins.ObjectPooler.Events.Core;

namespace Plugins.ObjectPooler.Events
{
    public class OnPooledObjectDisabled : PooledObjectEvent
    {
        public PooledObject pooledObject;

        #region MonoBehavior

        private void OnDisable()
        {
            Invoke(pooledObject);
        }

        #endregion
    }
}
