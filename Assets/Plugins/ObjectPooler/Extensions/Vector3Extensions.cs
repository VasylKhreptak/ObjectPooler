using UnityEngine;

namespace Plugins.ObjectPooler.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 ReplaceWithAxes(this Vector3 vector, UnityEngine.Vector3Int axes, Vector3 to)
        {
            if (axes.Is01() == false)
            {
                Debug.LogWarning("Axes is not correct!Method may work not as expected!");
            }

            Vector3 newVector = vector;
            if (axes.x == 1) newVector.x = to.x;
            if (axes.y == 1) newVector.y = to.y;
            if (axes.z == 1) newVector.z = to.z;

            return newVector;
        }
    }
}
