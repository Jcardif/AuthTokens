using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AuthTokens.Annotations;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AuthTokens
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<string> _graphScopes;
        private bool _useCustomScopes;
        private ObservableCollection<string> _selectedScopes = new ObservableCollection<string>();
        private string _accessToken;

        public string AccessToken
        {
            get => _accessToken;
            set => Set(ref _accessToken, value);
        }

        public ObservableCollection<string> SelectedScopes
        {
            get => _selectedScopes;
            set => Set(ref _selectedScopes, value);
        }

        public MainPage()
        {
            InitializeComponent();
            GraphScopes = new ObservableCollection<string>
            {
                "email", "profile", "User.Read", "offline_access", "openid", "User.Read.All", "User.ReadBasic.All",
                "User.ReadWrite", "User.ReadWrite.All"
            };


        }

        public ObservableCollection<string> GraphScopes
        {
            get => _graphScopes;
            set => Set(ref _graphScopes, value);
        }

        public bool UseCustomScopes
        {
            get => _useCustomScopes;
            set => Set(ref _useCustomScopes, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void AuthenticationTypeButtons_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

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

            if(!UseCustomScopes)
                GraphScopes.Add(scope);
        }

        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;

            if(toggleSwitch is null)
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

        private void GetAccessTokenButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}