using System;
using System.Collections;

namespace Microsoft.ManagementConsole.TaskMan
{
    public class TaskListSorter : IResultNodeComparer
    {
        private int columnIndex = -1;
        private readonly bool[] sortAsString = {
            true,  // 0 - Name
            false, // 1 - PID
            false, // 2 - PPID
            false, // 3 - CPU%
            false, // 4 - Private bytes
            false, // 5 - Working set
            true,  // 6 - Status
            true,  // 7 - User
            true,  // 8 - Commandline
        };

        int IComparer.Compare(object leftNode, object rightNode)
        {
            if (columnIndex < 0)
                return 0; // Trouble!

            var leftResultNode = (ResultNode)leftNode;
            var rightResultNode = (ResultNode)rightNode;
            var subItemColumnIndex = columnIndex - 1;
            if (sortAsString[columnIndex])
            {
                // The first column needs special handling. It's always a string.
                if (columnIndex == 0)
                    return string.Compare(
                    leftResultNode.DisplayName,
                    rightResultNode.DisplayName);

                // String sub-items.
                return string.Compare(
                    leftResultNode.SubItemDisplayNames[subItemColumnIndex],
                    rightResultNode.SubItemDisplayNames[subItemColumnIndex]);
            }
            else
            {
                // Numerical sub-items.
                var leftAsNumber = Convert.ToDouble(leftResultNode.SubItemDisplayNames[subItemColumnIndex]);
                var rightAsNumber = Convert.ToDouble(rightResultNode.SubItemDisplayNames[subItemColumnIndex]);
                if (leftAsNumber < rightAsNumber)
                    return -1;
                else if (leftAsNumber > rightAsNumber)
                    return 1;

                return 0; // Equal
            }
        }

        void IResultNodeComparer.SetColumnIndex(int index)
        {
            columnIndex = index;
        }
    }
} // namespace