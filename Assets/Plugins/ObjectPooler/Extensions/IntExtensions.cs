namespace Plugins.ObjectPooler.Extensions
{
    public static class IntExtensions
    {
        public static bool Is01(this int value)
        {
            return value == 0 || value == 1;
        }
    }
}
