using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Application.Services;
using PDV.Domain.Constants;
using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class EmployeesPage : Page
    {
        private readonly IAuthenticationService _authService;
        private readonly EmployeeService _employeeService;
        private readonly ILogger<EmployeesPage> _logger;
        private readonly NotificationService _notificationService;

        private Employee _currentUser;
        private ObservableCollection<Employee> _employees;

        public bool HasNoItems => _employees == null || !_employees.Any();

        public EmployeesPage()
        {
            this.InitializeComponent();

            // Obter servi�os da DI
            var app = Microsoft.UI.Xaml.Application.Current as App;
            _authService = app.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            _employeeService = app.Services.GetService(typeof(EmployeeService)) as EmployeeService;
            _logger = app.Services.GetService(typeof(ILogger<EmployeesPage>)) as ILogger<EmployeesPage>;
            _notificationService = NotificationService.Instance;

            // Inicializar cole��o vazia
            _employees = new ObservableCollection<Employee>();
            EmployeesList.ItemsSource = _employees;

            // Verificar permiss�es e carregar dados
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Carregar o usu�rio atual
                var settings = ApplicationData.Current.LocalSettings;
                string currentUsername = settings.Values["CurrentUsername"] as string ?? "admin";
                _currentUser = await _authService.GetEmployeeByUsernameAsync(currentUsername);

                if (_currentUser == null)
                {
                    _notificationService.ShowError("N�o foi poss�vel identificar o usu�rio atual");
                    return;
                }

                // Verificar permiss�o para visualizar funcion�rios
                if (!await _authService.HasPermissionAsync(_currentUser, Permissions.ViewEmployees))
                {
                    _notificationService.ShowError("Voc� n�o tem permiss�o para acessar a lista de funcion�rios");

                    // Desabilitar controles
                    EmployeesList.IsEnabled = false;
                    FilterBox.IsEnabled = false;
                    SearchBox.IsEnabled = false;

                    return;
                }

                // Carregar funcion�rios
                var employees = await _employeeService.GetAllEmployeesAsync();
                _employees.Clear();

                foreach (var employee in employees)
                {
                    _employees.Add(employee);
                }

                // Exibir ou ocultar o estado vazio
                EmptyState.Visibility = HasNoItems ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar funcion�rios");
                _notificationService.ShowError($"Erro ao carregar funcion�rios: {ex.Message}");
            }
        }

        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar permiss�o para adicionar funcion�rios
                if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.AddEmployee))
                {
                    _notificationService.ShowWarning("Voc� n�o tem permiss�o para adicionar funcion�rios");
                    return;
                }

                // Abrir di�logo para adicionar funcion�rio
                var dialog = new EmployeeDialog();
                dialog.XamlRoot = this.XamlRoot;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && dialog.CreatedEmployee != null)
                {
                    // Adicionar � lista
                    _employees.Add(dialog.CreatedEmployee);

                    // Selecionar o novo item
                    EmployeesList.SelectedItem = dialog.CreatedEmployee;

                    // Exibir ou ocultar o estado vazio
                    EmptyState.Visibility = HasNoItems ? Visibility.Visible : Visibility.Collapsed;

                    // Mostrar notifica��o
                    _notificationService.ShowSuccess("Funcion�rio adicionado com sucesso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar funcion�rio");
                _notificationService.ShowError($"Erro ao adicionar funcion�rio: {ex.Message}");
            }
        }

        private async void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Verificar permiss�o
            if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.EditEmployee))
            {
                _notificationService.ShowWarning("Voc� n�o tem permiss�o para editar funcion�rios");
                return;
            }

            // Verificar se h� um item selecionado
            var selectedEmployee = EmployeesList.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                _notificationService.ShowWarning("Selecione um funcion�rio para editar");
                return;
            }

            // Implementar l�gica de edi��o...
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Verificar permiss�o
            if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.DeleteEmployee))
            {
                _notificationService.ShowWarning("Voc� n�o tem permiss�o para excluir funcion�rios");
                return;
            }

            // Verificar se h� um item selecionado
            var selectedEmployee = EmployeesList.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                _notificationService.ShowWarning("Selecione um funcion�rio para excluir");
                return;
            }

            // Implementar l�gica de exclus�o...
        }

        private async void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Implementar l�gica de busca...
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementar l�gica de filtro...
        }

        private void EmployeesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedEmployee = EmployeesList.SelectedItem as Employee;
            if (selectedEmployee != null)
            {
                // Atualizar painel de detalhes
                NameText.Text = selectedEmployee.Name;
                RoleText.Text = selectedEmployee.Position;
                EmailText.Text = selectedEmployee.Email;
                PhoneText.Text = selectedEmployee.Phone;
                HireDateText.Text = selectedEmployee.HireDate.ToShortDateString();
            }
            else
            {
                // Limpar painel de detalhes
                NameText.Text = "-";
                RoleText.Text = "-";
                EmailText.Text = "-";
                PhoneText.Text = "-";
                HireDateText.Text = "-";
            }
        }
    }
}