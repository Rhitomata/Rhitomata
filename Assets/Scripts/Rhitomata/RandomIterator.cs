using System;
using Cysharp.Threading.Tasks;

namespace Rhitomata
{
    public static class RandomIterator
    {
        /// <summary>
        /// Asynchronously loops over a random range in chunks until count is reached.
        /// </summary>
        /// <param name="min">Minimum batch size</param>
        /// <param name="max">Maximum batch size</param>
        /// <param name="count">Total number of iterations</param>
        /// <param name="onLoop">Action to call on each iteration</param>
        /// <param name="onPostLoop">Action to call after each batch</param>
        public static async UniTask LoopAsync(
            int min,
            int max,
            int count,
            Action<int> onLoop,
            Func<UniTask> onPostLoop = null)
        {
            var i = 0;
            while (i < count)
            {
                var batchSize = UnityEngine.Random.Range(min, max);

                for (var j = 0; j < batchSize && i < count; j++, i++)
                {
                    onLoop?.Invoke(i);
                }

                if (onPostLoop != null)
                    await onPostLoop();
            }
        }
    }
}