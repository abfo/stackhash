using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;
using System.Collections;
using System.Windows.Media;

namespace StackHash
{
    /// <summary>
    /// Validates a WPF tree to determine if binding errors exist
    /// </summary>
    public static class BindingValidator
    {
        /// <summary>
        /// Validates a WPF tree to determine if binding errors exist
        /// </summary>
        /// <param name="parent">Parent DependencyObject</param>
        /// <returns>True if no errors exist</returns>
        public static bool IsValid(DependencyObject parent)
        {
            if (parent == null) { throw new ArgumentNullException("parent"); }

            // note that this goes wrong in a data template
            // http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/d88f3926-68e5-488d-9fb1-a9f94a5ffd11
            // http://stackoverflow.com/questions/338522/getlocalvalueenumerator-not-returning-all-properties

            bool valid = true;

            // validate parent
            LocalValueEnumerator localValueEnumerator = parent.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                LocalValueEntry entry = localValueEnumerator.Current;

                if (BindingOperations.IsDataBound(parent, entry.Property))
                {
                    Binding binding = BindingOperations.GetBinding(parent, entry.Property);
                    if (binding.Path.Path.Contains("Search"))
                    {
                        Debug.WriteLine("Search");
                    }
                    if ((binding.Mode == BindingMode.TwoWay) || (binding.Mode == BindingMode.OneWayToSource))
                    {
                        BindingExpression expression = BindingOperations.GetBindingExpression(parent, entry.Property);
                        expression.UpdateSource();
                        if (expression.HasError)
                        {
                            valid = false;
                        }
                    }
                }
            }

            // validate logical children
            bool foundLogicalChildren = false;
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object obj in children)
            {
                foundLogicalChildren = true;

                DependencyObject child = obj as DependencyObject;
                if (child != null)
                {
                    if (!IsValid(child))
                    {
                        valid = false;
                    }
                }
            }

            // if no logical children found try looking for visual children
            if ((!foundLogicalChildren) && (parent is Visual))
            {
                int visualChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
                if (visualChildrenCount > 0)
                {
                    for (int child = 0; child < visualChildrenCount; child++)
                    {
                        if (!IsValid(VisualTreeHelper.GetChild(parent, child)))
                        {
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }
    }
}
