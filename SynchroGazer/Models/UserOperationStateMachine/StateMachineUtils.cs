using System.Collections.Generic;

namespace Baku.SynchroGazer
{
    internal static class StateMachineUtils
    {
        public static void RefreshBlinkToggleQueue(
            Queue<BlinkToggleData> blinks, long timeStamp, long duration
            )
        {
            while (
                blinks.Count > 0 &&
                blinks.Peek().Time < timeStamp - duration
                )
            {
                blinks.Dequeue();
            }
        }

        //瞬き情報の一覧を参照し、要素中でClose, Openの繰り返しが何度出てくるかを計算します。
        public static int GetWinkCount(IEnumerable<BlinkToggleData> blinks)
        {
            int count = 0;
            bool searchTarget = false;

            foreach (var b in blinks)
            {
                if (b.IsOpened == searchTarget)
                {
                    searchTarget = !searchTarget;
                    if (b.IsOpened)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
