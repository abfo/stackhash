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
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Collection policy page for the setup wizard
    /// </summary>
    public partial class SetupCollectionPage : SetupBasePage
    {
        private bool _firstInitComplete;

        /// <summary>
        /// Collection policy page for the setup wizard
        /// </summary>
        public SetupCollectionPage()
        {
            InitializeComponent();

            this.BackEnabled = true;
            this.NextEnabled = true;
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public override void PageActivated()
        {
            if (!_firstInitComplete)
            {
                // find the current profile 
                StackHashContextSettings profile = null;

                foreach (DisplayContext contextSettings in this.ClientLogic.ContextCollection)
                {
                    if (contextSettings.Id == UserSettings.Settings.CurrentContextId)
                    {
                        profile = contextSettings.StackHashContextSettings;
                    }
                }

                // find and show the global collection policies
                StackHashCollectionPolicy globalCabPolicy = null;
                StackHashCollectionPolicy globalEventPolicy = null;
                
                foreach (StackHashCollectionPolicy policy in profile.CollectionPolicy)
                {
                    if ((policy.RootObject == StackHashCollectionObject.Global) &&
                        (policy.ObjectToCollect == StackHashCollectionObject.Cab))
                    {
                        if (policy.ConditionObject == StackHashCollectionObject.Cab)
                        {
                            globalCabPolicy = policy;
                        }
                        else if (policy.ConditionObject == StackHashCollectionObject.Event)
                        {
                            globalEventPolicy = policy;
                        }
                    }
                }

                collectionPolicyControl.SetPolicies(globalCabPolicy, globalEventPolicy, StackHashCollectionObject.Global, 0);

                _firstInitComplete = true;
            }

            this.ClientLogic.ClientLogicSetupWizardPrompt += new EventHandler<ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt);
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's no longer active
        /// </summary>
        public override void PageDeactivated()
        {
            this.ClientLogic.ClientLogicSetupWizardPrompt -= ClientLogic_ClientLogicSetupWizardPrompt;
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
            if (collectionPolicyControl.IsValid)
            {
                StackHashCollectionPolicyCollection policiesToUpdate;
                StackHashCollectionPolicyCollection policiesToRemove;
                collectionPolicyControl.UpdateAndReturnPolicies(out policiesToUpdate, out policiesToRemove);

                Debug.Assert(policiesToRemove.Count == 0);

                this.ClientLogic.SetCollectionPolicies(policiesToUpdate);
            }
        }

        void ClientLogic_ClientLogicSetupWizardPrompt(object sender, ClientLogicSetupWizardPromptEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.Prompt == ClientLogicSetupWizardPromptOperation.CollectionPoliciesSet)
                {
                    if (e.Succeeded)
                    {
                        this.DoRaiseShowNextPage();
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;

                        StackHashMessageBox.Show(Window.GetWindow(this),
                            Properties.Resources.SetupWizard_SetCollectionPoliciesFailedMBMessage,
                            Properties.Resources.SetupWizard_SetCollectionPoliciesFailedMBTitle,
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
    }
}
