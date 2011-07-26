using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHash
{
    /// <summary>
    /// Data point for a pie chart
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class PieChartDataPoint : IComparable<PieChartDataPoint>, IComparable
    {
        /// <summary>
        /// Gets or sets the data value for this point
        /// </summary>
        public double Data { get; set; }

        /// <summary>
        /// Gets or sets the label for this point
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Data point for a pie chart
        /// </summary>
        public PieChartDataPoint()
            : this(0.0, string.Empty) { }

        /// <summary>
        /// Data point for a pie chart
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="label">Label</param>
        public PieChartDataPoint(double data, string label)
        {
            this.Data = data;
            this.Label = label;
        }

        #region IComparable<PieChartDataPoint> Members

        /// <summary />
        public int CompareTo(PieChartDataPoint other)
        {
            if (other == null)
            {
                return -1;
            }
            else
            {
                // sort by data, descending
                int result = -this.Data.CompareTo(other.Data);
                if (result == 0)
                {
                    // if data the same sort by label
                    result = string.Compare(this.Label, other.Label, StringComparison.CurrentCulture);
                }
                return result;
            }
        }

        #endregion

        #region IComparable Members

        /// <summary />
        public int CompareTo(object obj)
        {
            PieChartDataPoint other = obj as PieChartDataPoint;
            if (other == null)
            {
                return -1;
            }
            else
            {
                return CompareTo(other);
            }
        }

        #endregion
    }
}
