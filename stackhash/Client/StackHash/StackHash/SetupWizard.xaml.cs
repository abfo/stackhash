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
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// StackHash Setup Wizard
    /// </summary>
    public partial class SetupWizard : Window
    {
        private SetupBasePage _currentPage;
        private int _currentPageIndex;
        private List<SetupBasePage> _pages;
        private ClientLogic _clientLogic;
        private bool _forceDisable;
        private bool _configuredForInitalSetup;

        /// <summary>
        /// True if the setup only selected the profile, false if full setup was run
        /// </summary>
        public bool SetupWasProfileOnly { get; private set; }

        /// <summary>
        /// StackHash Setup Wizard
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        public SetupWizard(ClientLogic clientLogic)
        {
            Debug.Assert(clientLogic != null);
            _clientLogic = clientLogic;

            InitializeComponent();

            _clientLogic.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_clientLogic_PropertyChanged);

            SetupWelcomePage welcomePage = new SetupWelcomePage();
            welcomePage.ClientLogic = _clientLogic;
            welcomePage.HelpTopic = "setup-welcome.htm";
            Grid.SetColumn(welcomePage, 2);

            _pages = new List<SetupBasePage>();
            _pages.Add(welcomePage);

            // default is regular setup - the welcome page will change this as needed
            ConfigureInitialSetup();

            SetPage(welcomePage);
        }

        private void ConfigureInitialSetup()
        {
            if (!_configuredForInitalSetup)
            {
                Debug.Assert(_currentPageIndex == 0);
                if (_currentPageIndex == 0)
                {
                    // remove all but the first page
                    while (_pages.Count > 1)
                    {
                        _pages.RemoveAt(1);
                    }
                }

                SetupAccountPage accountPage = new SetupAccountPage();
                accountPage.ClientLogic = _clientLogic;
                accountPage.HelpTopic = "setup-winqual.htm";
                Grid.SetColumn(accountPage, 2);

                SetupProductsPage productsPage = new SetupProductsPage();
                productsPage.ClientLogic = _clientLogic;
                productsPage.HelpTopic = "setup-products.htm";
                Grid.SetColumn(productsPage, 2);

                SetupCollectionPage collectionPage = new SetupCollectionPage();
                collectionPage.ClientLogic = _clientLogic;
                collectionPage.HelpTopic = "setup-collection.htm";
                Grid.SetColumn(collectionPage, 2);

                SetupFinishPage finishPage = new SetupFinishPage();
                finishPage.ClientLogic = _clientLogic;
                finishPage.HelpTopic = "setup-more.htm";
                Grid.SetColumn(finishPage, 2);

                _pages.Add(accountPage);
                _pages.Add(productsPage);
                _pages.Add(collectionPage);
                _pages.Add(finishPage);

                _configuredForInitalSetup = true;
                this.SetupWasProfileOnly = false;
            }
        }

        private void ConfigureProfileOnly()
        {
            if (_configuredForInitalSetup)
            {
                Debug.Assert(_currentPageIndex == 0);
                if (_currentPageIndex == 0)
                {
                    // remove all but the first page
                    while (_pages.Count > 1)
                    {
                        _pages.RemoveAt(1);
                    }
                }

                SetupProfilePage profilePage = new SetupProfilePage();
                profilePage.ClientLogic = _clientLogic;
                profilePage.HelpTopic = "setup-profile.htm";
                Grid.SetColumn(profilePage, 2);

                _pages.Add(profilePage);

                _configuredForInitalSetup = false;
                this.SetupWasProfileOnly = true;
            }
        }

        private void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (!_forceDisable)
                {
                    if (_clientLogic.NotBusy)
                    {
                        this.IsEnabled = true;
                        Mouse.OverrideCursor = null;
                    }
                    else
                    {
                        this.IsEnabled = false;
                        Mouse.OverrideCursor = Cursors.Wait;
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged),
                    sender, e);
            }
        }

        private void SetPage(SetupBasePage page)
        {
            Debug.Assert(page != null);

            DeactivateCurrentPage();

            gridWizard.Children.Add(page);
            this.DataContext = page;

            _currentPage = page;
            _currentPage.ShowNextPage += new EventHandler(_currentPage_ShowNextPage);
            _currentPage.ShowPreviousPage += new EventHandler(_currentPage_ShowPreviousPage);
            _currentPage.ConfigureForProfileOnly += new EventHandler(_currentPage_ConfigureForProfileOnly);
            _currentPage.ConfigureForInitialSetup += new EventHandler(_currentPage_ConfigureForInitialSetup);
            _currentPage.ExitWizard += new EventHandler<ExitWizardEventArgs>(_currentPage_ExitWizard);
            _currentPage.DisableEnableWizard += new EventHandler<DisableWizardEventArgs>(_currentPage_DisableEnableWizard);
            _currentPage.PageActivated();

            _currentPageIndex = _pages.IndexOf(_currentPage);

            if (_currentPageIndex == 0)
            {
                this.Title = Properties.Resources.SetupWizard_FirstPageTitle;
            }
            else
            {
                this.Title = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SetupWizard_Title,
                    _currentPageIndex + 1,
                    _pages.Count);
            }
        }

        private void DeactivateCurrentPage()
        {
            if (_currentPage != null)
            {
                gridWizard.Children.Remove(_currentPage);

                _currentPage.ShowNextPage -= _currentPage_ShowNextPage;
                _currentPage.ShowPreviousPage -= _currentPage_ShowPreviousPage;
                _currentPage.ConfigureForProfileOnly -= _currentPage_ConfigureForProfileOnly;
                _currentPage.ConfigureForInitialSetup -= _currentPage_ConfigureForInitialSetup;
                _currentPage.ExitWizard -= _currentPage_ExitWizard;
                _currentPage.DisableEnableWizard -= _currentPage_DisableEnableWizard;
                _currentPage.PageDeactivated();
            }
        }

        void _currentPage_DisableEnableWizard(object sender, DisableWizardEventArgs e)
        {
            if (e.ForceDisable)
            {
                this.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                _forceDisable = true;
            }
            else
            {
                this.IsEnabled = true;
                Mouse.OverrideCursor = null;
                _forceDisable = false;
            }
        }

        void _currentPage_ExitWizard(object sender, ExitWizardEventArgs e)
        {
            this.DialogResult = e.DialogResult;
        }

        void _currentPage_ConfigureForProfileOnly(object sender, EventArgs e)
        {
            ConfigureProfileOnly();
        }

        void _currentPage_ConfigureForInitialSetup(object sender, EventArgs e)
        {
            ConfigureInitialSetup();
        }

        void _currentPage_ShowPreviousPage(object sender, EventArgs e)
        {
            if (_currentPageIndex > 0)
            {
                SetPage(_pages[_currentPageIndex - 1]);
            }
        }

        void _currentPage_ShowNextPage(object sender, EventArgs e)
        {
            if (_currentPageIndex < (_pages.Count - 1))
            {
                SetPage(_pages[_currentPageIndex + 1]);
            }
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if ((_currentPage != null) && (_currentPage.BackEnabled))
            {
                _currentPage.TryBack();
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if ((_currentPage != null) && (_currentPage.NextEnabled))
            {
                _currentPage.TryNext();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DeactivateCurrentPage();
            _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
            Mouse.OverrideCursor = null;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_currentPage != null) && (_currentPage.HelpTopic != null))
            {
                StackHashHelp.ShowTopic(_currentPage.HelpTopic);
            }
            else
            {
                StackHashHelp.ShowTopic("setup.htm");
            }
        }
    }
}
