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

            // Obter serviços da DI
            var app = Microsoft.UI.Xaml.Application.Current as App;
            _authService = app.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            _employeeService = app.Services.GetService(typeof(EmployeeService)) as EmployeeService;
            _logger = app.Services.GetService(typeof(ILogger<EmployeesPage>)) as ILogger<EmployeesPage>;
            _notificationService = NotificationService.Instance;

            // Inicializar coleção vazia
            _employees = new ObservableCollection<Employee>();
            EmployeesList.ItemsSource = _employees;

            // Verificar permissões e carregar dados
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Carregar o usuário atual
                var settings = ApplicationData.Current.LocalSettings;
                string currentUsername = settings.Values["CurrentUsername"] as string ?? "admin";
                _currentUser = await _authService.GetEmployeeByUsernameAsync(currentUsername);

                if (_currentUser == null)
                {
                    _notificationService.ShowError("Não foi possível identificar o usuário atual");
                    return;
                }

                // Verificar permissão para visualizar funcionários
                if (!await _authService.HasPermissionAsync(_currentUser, Permissions.ViewEmployees))
                {
                    _notificationService.ShowError("Você não tem permissão para acessar a lista de funcionários");

                    // Desabilitar controles
                    EmployeesList.IsEnabled = false;
                    FilterBox.IsEnabled = false;
                    SearchBox.IsEnabled = false;

                    return;
                }

                // Carregar funcionários
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
                _logger.LogError(ex, "Erro ao carregar funcionários");
                _notificationService.ShowError($"Erro ao carregar funcionários: {ex.Message}");
            }
        }

        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar permissão para adicionar funcionários
                if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.AddEmployee))
                {
                    _notificationService.ShowWarning("Você não tem permissão para adicionar funcionários");
                    return;
                }

                // Abrir diálogo para adicionar funcionário
                var dialog = new EmployeeDialog();
                dialog.XamlRoot = this.XamlRoot;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && dialog.CreatedEmployee != null)
                {
                    // Adicionar à lista
                    _employees.Add(dialog.CreatedEmployee);

                    // Selecionar o novo item
                    EmployeesList.SelectedItem = dialog.CreatedEmployee;

                    // Exibir ou ocultar o estado vazio
                    EmptyState.Visibility = HasNoItems ? Visibility.Visible : Visibility.Collapsed;

                    // Mostrar notificação
                    _notificationService.ShowSuccess("Funcionário adicionado com sucesso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar funcionário");
                _notificationService.ShowError($"Erro ao adicionar funcionário: {ex.Message}");
            }
        }

        private async void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Verificar permissão
            if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.EditEmployee))
            {
                _notificationService.ShowWarning("Você não tem permissão para editar funcionários");
                return;
            }

            // Verificar se há um item selecionado
            var selectedEmployee = EmployeesList.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                _notificationService.ShowWarning("Selecione um funcionário para editar");
                return;
            }

            // Implementar lógica de edição...
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Verificar permissão
            if (_currentUser == null || !await _authService.HasPermissionAsync(_currentUser, Permissions.DeleteEmployee))
            {
                _notificationService.ShowWarning("Você não tem permissão para excluir funcionários");
                return;
            }

            // Verificar se há um item selecionado
            var selectedEmployee = EmployeesList.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                _notificationService.ShowWarning("Selecione um funcionário para excluir");
                return;
            }

            // Implementar lógica de exclusão...
        }

        private async void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Implementar lógica de busca...
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementar lógica de filtro...
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