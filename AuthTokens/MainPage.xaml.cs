using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Protection.PlayReady;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AuthTokens.Annotations;
using AuthTokens.Helpers;
using AuthTokens.Services;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AuthTokens
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private const string LoginConfigFolderName = "LoginConfig";

        private ObservableCollection<string> _graphScopes;
        private bool _useCustomScopes;
        private ObservableCollection<string> _selectedScopes = new ObservableCollection<string>();
        private string _accessToken;
        private string _tenantId;
        private string _clientId;
        private string _infoBarTitle;
        private string _infoBarMessage;
        private InfoBarSeverity _severity;
        private AuthenticationType _authenticationType;
        private ObservableCollection<AuthenticationType> _authenticationTypes;

        public string AccessToken
        {
            get => _accessToken;
            set => Set(ref _accessToken, value);
        }

        public ObservableCollection<string> SelectedScopes
        {
            get => _selectedScopes;
            set
            {
                Set(ref _selectedScopes, value);
            }
        }

        public string TenantId
        {
            get => _tenantId;
            set
            {
                Set(ref _tenantId, value);
            }
        }

        public string ClientId
        {
            get => _clientId;
            set
            {
                Set(ref _clientId, value);
            }
        }

        public string InfoBarTitle
        {
            get => _infoBarTitle;
            set => Set(ref _infoBarTitle, value);
        }

        public string InfoBarMessage
        {
            get => _infoBarMessage;
            set => Set(ref _infoBarMessage, value);
        }

        public InfoBarSeverity Severity
        {
            get => _severity;
            set => Set(ref _severity, value);
        }

        public AuthenticationType AuthenticationType
        {
            get => _authenticationType;
            set
            {
                _authenticationType = value;
            }
        }

        public ObservableCollection<string> GraphScopes
        {
            get => _graphScopes;
            set => Set(ref _graphScopes, value);
        }

        public bool UseCustomScopes
        {
            get => _useCustomScopes;
            set
            {
                Set(ref _useCustomScopes, value);
            }
        }

        public ObservableCollection<AuthenticationType> AuthenticationTypes
        {
            get => _authenticationTypes;
            set => Set(ref _authenticationTypes, value);
        }

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        public MainPage()
        {
            InitializeComponent();
            GraphScopes = new ObservableCollection<string>
            {
                "email", "profile", "User.Read", "offline_access", "openid", "User.Read.All", "User.ReadBasic.All",
                "User.ReadWrite", "User.ReadWrite.All"
            };

            InitializeLogIn();
        }

        private async void InitializeLogIn()
        {
            ClientId = await GetLoginSettings(LoginSettings.ClientId) as string;
            TenantId = await GetLoginSettings(LoginSettings.TenantId) as string;
            var useCustomScopes=await GetLoginSettings(LoginSettings.UseCustomScopes);
            if (useCustomScopes != null)
                UseCustomScopes = useCustomScopes is bool s && s;

            var authTypes = await GetLoginSettings(LoginSettings.AuthType) as List<AuthenticationType>;
            if (authTypes is null || authTypes.Count == 0)
            {
                AuthenticationTypes = new ObservableCollection<AuthenticationType>()
                {
                    new AuthenticationType(AuthenticationTypeEnum.MultipleOrgsOnly, "Multiple Organization Accounts Only", false),
                    new AuthenticationType(AuthenticationTypeEnum.SingleOrgOnly, "Single Organization Accounts Only", false),
                    new AuthenticationType(AuthenticationTypeEnum.PersonalAccountsOnly, "Personal Microsoft Accounts Only", false),
                    new AuthenticationType(AuthenticationTypeEnum.BothOrgAndPersonalAccounts,
                        "Both Organization and Personal Microsoft Accounts", false)
                };
            }
            else
            {
                AuthenticationTypes = new ObservableCollection<AuthenticationType>(authTypes);
            }


            if (await GetLoginSettings(LoginSettings.Scopes) is List<string> scopes)
                SelectedScopes = new ObservableCollection<string>(scopes);


            IdentityService.LoggedIn += OnLoggedIn;

            if(string.IsNullOrEmpty(ClientId))
                return;

            UserDataService.Initialize();
            IdentityService.InitializeWithAadAndPersonalMsAccounts(ClientId);
            var silentLoginSuccess = await IdentityService.AcquireTokenSilentAsync(SelectedScopes.ToList());
            if (!silentLoginSuccess || !IdentityService.IsAuthorized())
            {
                //await RedirectLoginPageAsync();
            }
            if (silentLoginSuccess && IdentityService.IsAuthorized())
                AccessToken = await IdentityService.GetAccessTokenAsync(SelectedScopes.ToList());
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            AccessToken = await IdentityService.GetAccessTokenAsync(SelectedScopes.ToList());
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return;

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void AddApiPermissionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CustomApiPermissionTextBox.Text))
                return;

            SelectedScopes.Add(CustomApiPermissionTextBox.Text);
            CustomApiPermissionTextBox.Text = string.Empty;
        }

        private void SelectedScopesListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var scope = e.ClickedItem.ToString();
            SelectedScopes.Remove(scope);

            if (!UseCustomScopes)
                GraphScopes.Add(scope);
        }

        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch is null)
                return;

            UseCustomScopes = toggleSwitch.IsOn;
            SelectedScopes = new ObservableCollection<string>();
            GraphScopes = new ObservableCollection<string>
            {
                "email", "profile", "User.Read", "offline_access", "openid", "User.Read.All", "User.ReadBasic.All",
                "User.ReadWrite", "User.ReadWrite.All"
            };
        }


        private void ScopesAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var scope = args.SelectedItem.ToString();
            if (string.IsNullOrEmpty(scope))
                return;
            sender.Text = string.Empty;

            SelectedScopes.Add(scope);
            GraphScopes.Remove(scope);
            sender.IsSuggestionListOpen = false;
        }

        private async void GetAccessTokenButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TenantId))
            {
                InfoBarTitle = "Tenant Id cannot be Empty";
                InfoBarMessage =
                    "Please provide the tenant Id to proceed, this can be obtained from Azure Ad or use the default value : Common";
                Severity = InfoBarSeverity.Error;
                return;
            }

            if (string.IsNullOrEmpty(ClientId))
            {
                InfoBarTitle = "Client Id cannot be Empty";
                InfoBarMessage =
                    "Please provide the client Id to proceed, this can be obtained from Azure Ad";
                Severity = InfoBarSeverity.Error;
                return;
            }

            if (SelectedScopes is null || SelectedScopes.Count == 0)
            {
                InfoBarTitle = "API Permissions missing";
                InfoBarMessage = "Provide either Graph API permissions or custom API Permissions";
                Severity = InfoBarSeverity.Error;
                return;
            }

            var first = AuthenticationTypes.FirstOrDefault(a => a.Name == AuthenticationType.Name);
            
            if (first != null) 
                first.IsChecked = true;

            AuthenticationTypes.Remove(AuthenticationType);
            AuthenticationTypes.Add(first);

            var config = new LoginConfig(ClientId, TenantId, UseCustomScopes, SelectedScopes.ToList(),
                AuthenticationTypes.ToList());
            await SaveLoginConfiguration(config);

            var loginResult = await IdentityService.LoginAsync(SelectedScopes.ToList());
        }

        private void CopyAccessTokenButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AccessToken))
                return;

            DataPackage dataPackage = new DataPackage();
            // copy 
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(AccessToken);
            Clipboard.SetContent(dataPackage);
        }

        private async void LogoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            await IdentityService.LogoutAsync();
        }

        private async Task SaveLoginConfiguration(LoginConfig config)
        {
            await ApplicationData.Current.LocalFolder.SaveAsync(LoginConfigFolderName, config);
        }

        private async Task<object> GetLoginSettings(LoginSettings setting)
        {
            var config = await ApplicationData.Current.LocalFolder.ReadAsync<LoginConfig>(LoginConfigFolderName);
            if (config is null)
                return null;

            switch (setting)
            {
                case LoginSettings.ClientId:
                    return config.ClientId;
                    
                case LoginSettings.TenantId:
                    return config.TenantId;
                    
                case LoginSettings.AuthType:
                    return config.AuthenticationTypes;

                case LoginSettings.UseCustomScopes:
                    return config.UseCustomScopes;

                case LoginSettings.Scopes:
                    return config.Scopes;

                default:
                    throw new ArgumentOutOfRangeException(nameof(setting), setting, null);
            }

        }
    }
}