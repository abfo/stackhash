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
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Control to specify the display (filter) policy
    /// </summary>
    public partial class DisplayPolicyControl : UserControl
    {
        private class ContextValidation : IDataErrorInfo
        {
            public bool ValidationEnabled { get; set; }
            public int HitThreshold { get; set; }

            public ContextValidation(int hitThreshold)
            {
                this.HitThreshold = hitThreshold;
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
                        case "HitThreshold":
                            if (HitThreshold < 0)
                            {
                                result = Properties.Resources.DisplayPolicyControl_ValidationErrorHitThreshold;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private ContextValidation _contextValidation;
        private bool _isGlobal;

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
        /// Control to specify the display (filter) policy
        /// </summary>
        public DisplayPolicyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the hit threshold
        /// </summary>
        /// <param name="hitThreshold">Hit threshold</param>
        /// <param name="isGlobal">True if this is the global threshold</param>
        /// <param name="isOverride">True if the provided value is an override for a product</param>
        public void SetHitThreshold(int hitThreshold, bool isGlobal, bool isOverride)
        {
            _contextValidation = new ContextValidation(hitThreshold);
            this.DataContext = _contextValidation;

            _isGlobal = isGlobal;

            if (_isGlobal)
            {
                textDisplayFilter.Visibility = Visibility.Visible;
                checkBoxDisplayFilter.Visibility = Visibility.Hidden;
            }
            else
            {
                textDisplayFilter.Visibility = Visibility.Hidden;
                checkBoxDisplayFilter.Visibility = Visibility.Visible;
            }

            checkBoxDisplayFilter.IsChecked = isOverride;

            UpdateState();
        }

        /// <summary>
        /// Gets the current hit threshold - call IsValid first to ensure
        /// that the value passes validation
        /// </summary>
        /// <returns>The current hit threshold, -1 if any threshold should be removed</returns>
        public int GetHitThreshold()
        {
            int hitThreshold = -1;

            if (this.IsValid)
            {
                if (_isGlobal || (checkBoxDisplayFilter.IsChecked == true))
                {
                    hitThreshold = _contextValidation.HitThreshold;
                }
            }

            return hitThreshold;
        }

        private void UpdateState()
        {
            if (checkBoxDisplayFilter.IsChecked == true)
            {
                labelDisplayHitThreshold.IsEnabled = true;
                textBoxDisplayHitThreshold.IsEnabled = true;
            }
            else
            {
                labelDisplayHitThreshold.IsEnabled = false;
                textBoxDisplayHitThreshold.IsEnabled = false;
            }
        }

        private void checkBoxDisplayFilter_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxDisplayFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }
    }
}
