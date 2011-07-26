using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Collections;
using System.Windows.Media;
using System.Windows.Documents;

namespace StackHash
{
    /// <summary>
    /// Implements sorting for a ListView with a data bound GridView. Sorting is based on the binding path
    /// set in DisplayMemberBinding OR uses the Tag set on the GridViewColumnHeader
    /// </summary>
    public class ListViewSorter
    {
        private ListView _listView;
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private SortDirectionAdorner _currentAdorner;
        private List<SortDescription> _defaultDescriptions;

        /// <summary>
        /// Implements sorting for a ListView with a data bound GridView. Sorting is based on the binding path
        /// set in DisplayMemberBinding OR uses the Tag set on the GridViewColumnHeader
        /// </summary>
        /// <param name="listView">The ListView</param>
        public ListViewSorter(ListView listView)
        {
            Debug.Assert(listView != null);
            _listView = listView;
            _lastDirection = ListSortDirection.Descending;
            _defaultDescriptions = new List<SortDescription>();
        }

        /// <summary>
        /// Adds a default sort, applied in the order added
        /// </summary>
        /// <param name="bindingPath">The path to the property to sort</param>
        /// <param name="direction">The direction to sort in</param>
        public void AddDefaultSort(string bindingPath, ListSortDirection direction)
        {            
            _defaultDescriptions.Add(new SortDescription(bindingPath, direction));
        }

        /// <summary>
        /// Repeats the last sort, applies default sort orders if a header has not been clicked previously
        /// </summary>
        public void SortLastColumn()
        {
            if (_lastHeaderClicked != null)
            {
                SortColumnCore(_lastHeaderClicked, _lastDirection);
            }
            else
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(_listView.ItemsSource);
                if (dataView != null)
                {
                    using (dataView.DeferRefresh())
                    {
                        dataView.SortDescriptions.Clear();

                        foreach (SortDescription sort in _defaultDescriptions)
                        {
                            dataView.SortDescriptions.Add(sort);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts a column
        /// </summary>
        /// <param name="header">The GridViewColumnHeader of the column to sort</param>
        public void SortColumn(GridViewColumnHeader header)
        {
            if (header != null)
            {
                // don't sort if the user clicked the padding area of the header, or if there is no column to sort
                if ((header.Role != GridViewColumnHeaderRole.Padding) && (header.Column != null))
                {
                    ListSortDirection direction;

                    if (header != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    SortColumnCore(header, direction);
                }
            }
        }

        private void SortColumnCore(GridViewColumnHeader header, ListSortDirection direction)
        {
            // get the binding path for the column associated with the clicked header
            string bindingPath;
            if (header.Column.DisplayMemberBinding == null)
            {
                // if DisplayMemberBinding is not set then the path should be the tag for the column
                bindingPath = header.Tag as string;
            }
            else
            {
                // id DisplayMemberBinding is set then get the binding path
                Binding columnBinding = header.Column.DisplayMemberBinding as Binding;
                Debug.Assert(columnBinding != null);
                bindingPath = columnBinding.Path.Path;
            }

            ICollectionView dataView = CollectionViewSource.GetDefaultView(_listView.ItemsSource);
            if (dataView != null)
            {
                using (dataView.DeferRefresh())
                {
                    dataView.SortDescriptions.Clear();
                    dataView.SortDescriptions.Add(new SortDescription(bindingPath, direction));
                    ApplyRemainingDefaults(ref dataView, bindingPath);
                }
            }

            if ((_lastHeaderClicked != null) && (_currentAdorner != null))
            {
                AdornerLayer.GetAdornerLayer(_lastHeaderClicked).Remove(_currentAdorner);
            }

            _currentAdorner = new SortDirectionAdorner(header, direction);
            AdornerLayer.GetAdornerLayer(header).Add(_currentAdorner);

            _lastHeaderClicked = header;
            _lastDirection = direction;
        }

        private void ApplyRemainingDefaults(ref ICollectionView dataView, string bindingPath)
        {
            if (_defaultDescriptions.Count > 0)
            {
                // find the index that the user is sorting
                int bindingPathIndex = -1;
                for (int i = 0; i < _defaultDescriptions.Count; i++)
                {
                    if (string.Compare(_defaultDescriptions[i].PropertyName, bindingPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bindingPathIndex = i;
                        break;
                    }
                }

                // add the remaining defaults
                for (int i = 0; i < _defaultDescriptions.Count; i++)
                {
                    if (i != bindingPathIndex)
                    {
                        dataView.SortDescriptions.Add(_defaultDescriptions[i]);
                    }
                }
            }
        }
    }
}
