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
using System.Collections.ObjectModel;

namespace StackHash
{
    /// <summary>
    /// Profile page for the setup wizard (used to just select the profile and exit)
    /// </summary>
    public partial class SetupProfilePage : SetupBasePage
    {
        /// <summary>
        /// Profile page for the setup wizard (used to just select the profile and exit)
        /// </summary>
        public SetupProfilePage()
        {
            InitializeComponent();

            this.NextEnabled = true;
            this.BackEnabled = true;
            this.NextText = Properties.Resources.ButtonText_Finish;
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public override void PageActivated()
        {
            ObservableCollection<DisplayContext> activeContexts = new ObservableCollection<DisplayContext>();
            if (this.ClientLogic.ContextCollection != null)
            {
                foreach (DisplayContext context in this.ClientLogic.ContextCollection)
                {
                    if (context.IsActive)
                    {
                        activeContexts.Add(context);
                    }
                }
            }
            comboBoxProfile.ItemsSource = activeContexts;
        }

        /// <summary>
        /// Request navigation to the previous page - ShowPreviousPage will fire if this is successfull
        /// </summary>
        public override void TryBack()
        {
            // can always go back
            DoRaiseShowPreviousPage();
        }

        /// <summary>
        /// Request navigation to the next page - ShowNextPage will fire if this is successfull
        /// </summary>
        public override void TryNext()
        {
            int selectedContextId = UserSettings.InvalidContextId;

            DisplayContext context = comboBoxProfile.SelectedItem as DisplayContext;
            if (context != null)
            {
                selectedContextId = context.Id;
            }

            UserSettings.Settings.CurrentContextId = selectedContextId;

            // exit the wizard
            DoRaiseExitWizard(true);
        }
    }
}
