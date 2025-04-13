using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private Dictionary<string, Type> _pages;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomHeader);
            NotificationService.Instance.Initialize(GlobalNotification);
            InitializePages();
            SetupEvents();
            UpdateDateDisplay();
        }

        private void InitializePages()
        {
            _pages = new Dictionary<string, Type>
            {
                { "login", typeof(LoginPage) },
                { "home", typeof(HomePage) },
                { "employees", typeof(EmployeesPage) },
                { "products", typeof(ProductsPage) },
                { "reports", typeof(ReportsPage) },
                { "pos", typeof(POSPage) }
            };
        }

        private void SetupEvents()
        {
            SessionService.Instance.UserLoggedIn += SessionService_UserLoggedIn;
            SessionService.Instance.UserLoggedOut += SessionService_UserLoggedOut;
        }

        private void SessionService_UserLoggedIn(object sender, EventArgs e)
        {
            CustomHeader.Visibility = Visibility.Visible;
            SetTitleBar(CustomHeader);

            UpdateUserInfo();
        }

        private void SessionService_UserLoggedOut(object sender, EventArgs e)
        {
            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
            NavView.IsPaneVisible = false;
        }

        private void UpdateUserInfo()
        {
            var user = SessionService.Instance.CurrentUser;
            if (user != null)
            {
                UserNameTextBlock.Text = user.Name;
                UserInitialsControl.Initials = GetInitials(user.Name);

                NavView.IsPaneVisible = true;

                UpdateNavViewBasedOnPermissions();
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            else
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private void UpdateNavViewBasedOnPermissions()
        {
            // Hide/show navigation items based on user permissions
            // This would be implemented based on the permissions system
        }

        private void UpdateDateDisplay()
        {
        }

        public void NavigateToHomePage()
        {
            NavView.IsPaneVisible = true;

            ContentFrame.Navigate(typeof(HomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            foreach (NavigationViewItem item in NavView.MenuItems)
            {
                if (item.Tag?.ToString() == "home")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            UpdateUserInfo();
        }

        public void NavigateToLoginPage()
        {
            NavView.IsPaneVisible = false;

            CustomHeader.Visibility = Visibility.Collapsed;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);

            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString();

                if (tag == "sync")
                {
                    SyncButton_Tapped(null, null);
                    return;
                }

                if (_pages.TryGetValue(tag, out Type pageType))
                {
                    ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            SessionService.Instance.Logout();

            NotificationService.Instance.ShowInformation("Logout realizado com sucesso.");

            NavigateToLoginPage();
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            // Any logic needed when navigation pane opens
        }

        private void SyncButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NotificationService.Instance.ShowInformation("Sincronização iniciada. Aguarde...");

            DispatcherQueue.TryEnqueue(async () =>
            {
                NotificationService.Instance.ShowSuccess("Sincronização concluída com sucesso!");
            });
        }
    }
}