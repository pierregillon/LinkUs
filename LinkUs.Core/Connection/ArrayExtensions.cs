using System.Linq;

namespace LinkUs.Core.Connection
{
    public static class ArrayExtensions
    {
        public static T[] SmartSkip<T>(this T[] array, int count)
        {
            if (count == 0) {
                return array;
            }
            else {
                return array.Skip(count).ToArray();
            }
        }

        public static T[] SmartTake<T>(this T[] array, int count)
        {
            if (count == array.Length) {
                return array;
            }
            else {
                return array.Take(count).ToArray();
            }
        }
    }
}