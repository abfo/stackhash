using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StackHash.StackHashService;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Control specifying cab and event collection policies 
    /// </summary>
    public partial class CollectionPolicyControl : UserControl
    {
        private class ContextValidation : IDataErrorInfo
        {
            public class DisplayCollectionOrder
            {
                public StackHashCollectionOrder CollectionOrder { get; set; }
                private string _displayString;

                public DisplayCollectionOrder(StackHashCollectionOrder collectionOrder, string displayString)
                {
                    this.CollectionOrder = collectionOrder;
                    _displayString = displayString;
                }

                public static ObservableCollection<DisplayCollectionOrder> GetCollectionOrders()
                {
                    ObservableCollection<DisplayCollectionOrder> collectionOrders = new ObservableCollection<DisplayCollectionOrder>();
                    collectionOrders.Add(new DisplayCollectionOrder(StackHashCollectionOrder.LargestFirst, Properties.Resources.CollectionOrder_LargestFirst));
                    collectionOrders.Add(new DisplayCollectionOrder(StackHashCollectionOrder.SmallestFirst, Properties.Resources.CollectionOrder_SmallestFirst));
                    collectionOrders.Add(new DisplayCollectionOrder(StackHashCollectionOrder.MostRecentFirst, Properties.Resources.CollectionOrder_MostRecentFirst));
                    collectionOrders.Add(new DisplayCollectionOrder(StackHashCollectionOrder.OldestFirst, Properties.Resources.CollectionOrder_OldestFirst));
                    collectionOrders.Add(new DisplayCollectionOrder(StackHashCollectionOrder.AsReceived, Properties.Resources.CollectionOrder_AsReceived));
                    return collectionOrders;
                }

                public override string ToString()
                {
                    return _displayString;
                }
            }

            public bool ValidationEnabled { get; set; }
            public int CabPercentage { get; set; }
            public int CabCount { get; set; }
            public int EventHitThreshold { get; set; }
            public bool CabCollectLarger { get; set; }
            public DisplayCollectionOrder SelectedCollectionOrder { get; set; }
            public ObservableCollection<DisplayCollectionOrder> CollectionOrders { get; private set; }

            public ContextValidation(int cabPercentage, 
                int cabCount, 
                int eventHitThreshold, 
                bool cabCollectLarger, 
                StackHashCollectionOrder cabCollectionOrder)
            {
                this.CabPercentage = cabPercentage;
                this.CabCount = cabCount;
                this.EventHitThreshold = eventHitThreshold;
                this.CabCollectLarger = cabCollectLarger;
                this.CollectionOrders = DisplayCollectionOrder.GetCollectionOrders();

                foreach(DisplayCollectionOrder order in this.CollectionOrders)
                {
                    if (order.CollectionOrder == cabCollectionOrder)
                    {
                        this.SelectedCollectionOrder = order;
                        break;
                    }
                }

                Debug.Assert(this.SelectedCollectionOrder != null);
            }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get
                {
                    if (!ValidationEnabled)
                    {
                        return null;
                    }

                    string result = null;

                    switch (columnName)
                    {
                        case "CabPercentage":
                            if ((CabPercentage < 0) || (CabPercentage > 100))
                            {
                                result = Properties.Resources.CollectionPolicyControl_ValidationErrorCabPercentage;
                            }
                            break;

                        case "CabCount":
                            if (CabCount < 0)
                            {
                                result = Properties.Resources.CollectionPolicyControl_ValidationErrorCabCount;
                            }
                            break;

                        case "EventHitThreshold":
                            if (CabCount < 0)
                            {
                                result = Properties.Resources.CollectionPolicyControl_ValidationErrorHitThreshold;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private StackHashCollectionPolicy _cabPolicy;
        private StackHashCollectionPolicy _eventPolicy;
        private ContextValidation _contextValidation;
        private StackHashCollectionObject _level;
        private bool _providedCabPolicyIsAtThisLevel;
        private bool _providedEventPolicyIsAtThislevel;
        private int _rootIdForOverride;

        /// <summary>
        /// Control specifying cab and event collection policies 
        /// </summary>
        public CollectionPolicyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// True if the control is currently in a valid state
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_contextValidation == null)
                {
                    return false;
                }
                else
                {
                    _contextValidation.ValidationEnabled = true;
                    return BindingValidator.IsValid(this);
                }
            }
        }

        /// <summary>
        /// Sets the policies used by the control
        /// </summary>
        /// <param name="cabPolicy">Cab collection policy</param>
        /// <param name="eventPolicy">Event collection policy (optional)</param>
        /// <param name="level">The actual level of this CollectionPolicyControl</param>
        /// <param name="overrideId">The Id of the object that will apply to any possible override</param>
        public void SetPolicies(StackHashCollectionPolicy cabPolicy, StackHashCollectionPolicy eventPolicy, StackHashCollectionObject level, int overrideId)
        {
            if (cabPolicy == null) { throw new ArgumentNullException("cabPolicy"); }
            // OK for eventPolicy to be null

            _cabPolicy = cabPolicy;
            _level = level;
            _rootIdForOverride = overrideId;

            int hitThreshold = 0;

            // use labels / checkboxes depending on level
            if (level == StackHashCollectionObject.Global)
            {
                textCabCollectionPolicy.Visibility = Visibility.Visible;
                textEventPolicy.Visibility = Visibility.Visible;
                checkBoxCabCollectionPolicy.Visibility = Visibility.Collapsed;
                checkBoxEventCollectionPolicy.Visibility = Visibility.Collapsed;
            }
            else
            {
                textCabCollectionPolicy.Visibility = Visibility.Collapsed;
                textEventPolicy.Visibility = Visibility.Collapsed;
                checkBoxCabCollectionPolicy.Visibility = Visibility.Visible;
                checkBoxEventCollectionPolicy.Visibility = Visibility.Visible;
            }

            // hide event policy if not specified
            if (eventPolicy == null)
            {
                _eventPolicy = null;

                textEventPolicy.Visibility = Visibility.Collapsed;
                labelHitThreshold.Visibility = Visibility.Collapsed;
                textBoxEventHitThreshold.Visibility = Visibility.Collapsed;
                checkBoxEventCollectionPolicy.Visibility = Visibility.Collapsed;
            }
            else
            {
                _eventPolicy = eventPolicy;

                hitThreshold = _eventPolicy.Minimum;

                labelHitThreshold.Visibility = Visibility.Visible;
                textBoxEventHitThreshold.Visibility = Visibility.Visible;
            }

            _contextValidation = new ContextValidation(_cabPolicy.Percentage, 
                _cabPolicy.Maximum, 
                hitThreshold, 
                _cabPolicy.CollectLarger,
                _cabPolicy.CollectionOrder);
            this.DataContext = _contextValidation;

            switch (_cabPolicy.CollectionType)
            {
                case StackHashCollectionType.All:
                default:
                    radioButtonAll.IsChecked = true;
                    break;

                case StackHashCollectionType.Count:
                    radioButtonCount.IsChecked = true;
                    break;

                case StackHashCollectionType.Percentage:
                    radioButtonPercentage.IsChecked = true;
                    break;

                case StackHashCollectionType.None:
                    radioButtonNone.IsChecked = true;
                    break;
            }

            // determine if the policies are set at this level or inherited
            if (_cabPolicy.RootObject == _level)
            {
                checkBoxCabCollectionPolicy.IsChecked = true;
                _providedCabPolicyIsAtThisLevel = true;
            }

            if ((_eventPolicy != null) && (_eventPolicy.RootObject == _level))
            {
                checkBoxEventCollectionPolicy.IsChecked = true;
                _providedEventPolicyIsAtThislevel = true;
            }
        }

        /// <summary>
        /// Updates and returns the current cab policy object
        /// </summary>
        /// <returns>StackHashCollectionPolicy</returns>
        [Obsolete("Use UpdateAndReturnPolicies()")]
        public StackHashCollectionPolicy UpdateAndReturnCabPolicy()
        {
            StackHashCollectionPolicy cabPolicy = null;

            if (this.IsValid)
            {
                _cabPolicy.CollectLarger = _contextValidation.CabCollectLarger;
                _cabPolicy.Maximum = _contextValidation.CabCount;
                _cabPolicy.Percentage = _contextValidation.CabPercentage;
                _cabPolicy.CollectionOrder = _contextValidation.SelectedCollectionOrder.CollectionOrder;

                cabPolicy = _cabPolicy;
            }

            return cabPolicy;
        }

        /// <summary>
        /// Updates and returns the current event policy object
        /// </summary>
        /// <returns>StackHashCollectionPolicy</returns>
        [Obsolete("Use UpdateAndReturnPolicies()")]
        public StackHashCollectionPolicy UpdateAndReturnEventPolicy()
        {
            StackHashCollectionPolicy eventPolicy = null;

            if ((this.IsValid) && (_eventPolicy != null))
            {
                _eventPolicy.Minimum = _contextValidation.EventHitThreshold;

                eventPolicy = _eventPolicy;
            }

            return eventPolicy;
        }

        /// <summary>
        /// Updates and returns the current cab and (if present) event policies
        /// </summary>
        /// <param name="policiesToUpdate">Returns policies to set - may be empty</param>
        /// <param name="policiesToRemove">Returns policies to remove - may be empty</param>
        public void UpdateAndReturnPolicies(out StackHashCollectionPolicyCollection policiesToUpdate, out StackHashCollectionPolicyCollection policiesToRemove)
        {
            policiesToUpdate = new StackHashCollectionPolicyCollection();
            policiesToRemove = new StackHashCollectionPolicyCollection();

            if (this.IsValid)
            {
                // update/remove cab policy
                if (checkBoxCabCollectionPolicy.IsChecked == true)
                {
                    // update cab policy
                    _cabPolicy.CollectLarger = _contextValidation.CabCollectLarger;
                    _cabPolicy.Maximum = _contextValidation.CabCount;
                    _cabPolicy.Percentage = _contextValidation.CabPercentage;
                    _cabPolicy.CollectionOrder = _contextValidation.SelectedCollectionOrder.CollectionOrder;
                    _cabPolicy.RootObject = _level;
                    _cabPolicy.RootId = _rootIdForOverride;

                    policiesToUpdate.Add(_cabPolicy);
                }
                else
                {
                    if (_providedCabPolicyIsAtThisLevel)
                    {
                        policiesToRemove.Add(_cabPolicy);
                    }
                }
                
                // update/remove event policy (if one was provided
                if (_eventPolicy != null)
                {
                    if (checkBoxEventCollectionPolicy.IsChecked == true)
                    {
                        _eventPolicy.Minimum = _contextValidation.EventHitThreshold;
                        _eventPolicy.RootObject = _level;
                        _eventPolicy.RootId = _rootIdForOverride;

                        policiesToUpdate.Add(_eventPolicy);
                    }
                    else
                    {
                        if (_providedEventPolicyIsAtThislevel)
                        {
                            policiesToRemove.Add(_eventPolicy);
                        }
                    }
                }
            }
        }

        private void UpdateState()
        {
            if (checkBoxCabCollectionPolicy.IsChecked == true)
            {
                radioButtonAll.IsEnabled = true;
                radioButtonCount.IsEnabled = true;
                radioButtonPercentage.IsEnabled = true;
                radioButtonNone.IsEnabled = true;

                // collection order and download larger only make sense if the number of cabs is limited
                if ((radioButtonAll.IsChecked == true) || (radioButtonNone.IsChecked == true))
                {
                    comboBoxCollectionorder.IsEnabled = false;
                    checkBoxDownloadLarger.IsEnabled = false;
                }
                else
                {
                    comboBoxCollectionorder.IsEnabled = true;
                    checkBoxDownloadLarger.IsEnabled = true;
                }

                textBoxMaximum.IsEnabled = (radioButtonCount.IsChecked == true);
                textBoxPercentage.IsEnabled = (radioButtonPercentage.IsChecked == true);
            }
            else
            {
                radioButtonAll.IsEnabled = false;
                radioButtonCount.IsEnabled = false;
                textBoxMaximum.IsEnabled = false;
                radioButtonPercentage.IsEnabled = false;
                textBoxPercentage.IsEnabled = false;
                radioButtonNone.IsEnabled = false;
                comboBoxCollectionorder.IsEnabled = false;
                checkBoxDownloadLarger.IsEnabled = false;
            }

            if (checkBoxEventCollectionPolicy.IsChecked == true)
            {
                labelHitThreshold.IsEnabled = true;
                textBoxEventHitThreshold.IsEnabled = true;
            }
            else
            {
                labelHitThreshold.IsEnabled = false;
                textBoxEventHitThreshold.IsEnabled = false;
            }
        }

        private void radioButtonAll_Checked(object sender, RoutedEventArgs e)
        {
            _cabPolicy.CollectionType = StackHashCollectionType.All;
            UpdateState();
        }

        private void radioButtonCount_Checked(object sender, RoutedEventArgs e)
        {
            _cabPolicy.CollectionType = StackHashCollectionType.Count;
            UpdateState();
        }

        private void radioButtonPercentage_Checked(object sender, RoutedEventArgs e)
        {
            _cabPolicy.CollectionType = StackHashCollectionType.Percentage;
            UpdateState();
        }

        private void radioButtonNone_Checked(object sender, RoutedEventArgs e)
        {
            _cabPolicy.CollectionType = StackHashCollectionType.None;
            UpdateState();
        }

        private void checkBoxCabCollectionPolicy_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxCabCollectionPolicy_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxEventCollectionPolicy_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxEventCollectionPolicy_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }
    }
}
