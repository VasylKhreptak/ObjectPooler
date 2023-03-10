namespace Plugins.ObjectPooler.Extensions
{
    public static class Vector3Int
    {
        public static bool Is01(this UnityEngine.Vector3Int vector)
        {
            if (vector.x.Is01() == false) return false;
            if (vector.y.Is01() == false) return false;
            if (vector.z.Is01() == false) return false;
            return true;
        }
    }
}
