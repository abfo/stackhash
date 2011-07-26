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
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Final page for the setup wizard
    /// </summary>
    public partial class SetupFinishPage : SetupBasePage
    {
        private bool _initComplete;

        /// <summary>
        /// Final page for the setup wizard
        /// </summary>
        public SetupFinishPage()
        {
            InitializeComponent();

            this.NextEnabled = true;
            this.BackEnabled = true;
            this.NextText = Properties.Resources.ButtonText_Finish;
        }

        /// <summary />
        public override void PageActivated()
        {
            if (!_initComplete)
            {
                displayPolicyControl.SetHitThreshold(UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId), true, true);
                _initComplete = true;
            }

            this.ClientLogic.ClientLogicSetupWizardPrompt += new EventHandler<ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt);
        }

        /// <summary />
        public override void PageDeactivated()
        {
            this.ClientLogic.ClientLogicSetupWizardPrompt -= ClientLogic_ClientLogicSetupWizardPrompt;
        }

        /// <summary>
        /// Request navigation to the next page - ShowNextPage will fire if this is successfull
        /// </summary>
        public override void TryNext()
        {
            if (displayPolicyControl.IsValid)
            {
                UserSettings.Settings.SetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId,
                    displayPolicyControl.GetHitThreshold());

                this.ClientLogic.AdminUpdateReportingState(false);
            }
        }

        /// <summary>
        /// Request navigation to the previous page - ShowPreviousPage will fire if this is successfull
        /// </summary>
        public override void TryBack()
        {
            // can always go back
            DoRaiseShowPreviousPage();
        }

        void ClientLogic_ClientLogicSetupWizardPrompt(object sender, ClientLogicSetupWizardPromptEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.Prompt == ClientLogicSetupWizardPromptOperation.ReportingUpdated)
                {
                    if (e.Succeeded)
                    {
                        DoRaiseExitWizard(true);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;

                        StackHashMessageBox.Show(Window.GetWindow(this),
                            Properties.Resources.SetupWizard_UpdateReportingStateFailedMBMessage,
                            Properties.Resources.SetupWizard_UpdateReportingStateFailedMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error,
                            e.LastException,
                            e.LastServiceError);
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt),
                    sender, e);
            }
        }

        private void hyperlinkServiceOptions_Click(object sender, RoutedEventArgs e)
        {
            if (this.ClientLogic.ServiceIsLocal)
            {
                ProfileManager profileManager = new ProfileManager(this.ClientLogic);
                profileManager.Owner = Window.GetWindow(this);
                profileManager.ShowDialog();
            }
            else
            {
                StackHashMessageBox.Show(Window.GetWindow(this),
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ServiceOptionsUnavailable_MBMessage,
                    this.ClientLogic.ServiceHost),
                    Properties.Resources.ServiceOptionsUnavailable_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information);
            }
        }

        private void hyperlinkClientOptions_Click(object sender, RoutedEventArgs e)
        {
            if (displayPolicyControl.IsValid)
            {
                UserSettings.Settings.SetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId,
                    displayPolicyControl.GetHitThreshold());

                OptionsWindow optionsWindow = new OptionsWindow(this.ClientLogic);
                optionsWindow.Owner = Window.GetWindow(this);
                optionsWindow.ShowDialog();

                // in case the user changed settings
                displayPolicyControl.SetHitThreshold(UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId), true, true);
            }
        }
    }
}
