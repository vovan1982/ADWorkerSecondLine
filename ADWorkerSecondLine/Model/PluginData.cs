using System.Collections.Generic;
using System.Windows.Controls;

namespace ADWorkerSecondLine.Model
{
    public class PluginData
    {
        public int IndexNumber { get; set; }
        public string DisplayName { get; set; }
        public UserControl PLControl { get; set; }
    }

    public class PluginDataComparer : IComparer<PluginData>
    {
        public int Compare(PluginData x, PluginData y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    if (x.IndexNumber > y.IndexNumber)
                        return 1;
                    else if (x.IndexNumber < y.IndexNumber)
                        return -1;
                    else
                        return 0;
                }
            }

        }
    }
}
