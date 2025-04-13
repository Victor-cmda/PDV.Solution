using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Application.DTOs;
using PDV.Application.Services;
using PDV.Domain.Constants;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class EmployeeDialog : ContentDialog
    {
        private readonly IAuthenticationService _authService;
        private readonly EmployeeService _employeeService;
        private Employee _currentUser;
        private bool _isEditMode;

        public Employee CreatedEmployee { get; private set; }

        public EmployeeDialog()
        {
            this.InitializeComponent();

            var app = Microsoft.UI.Xaml.Application.Current as App;
            _authService = app.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            _employeeService = app.Services.GetService(typeof(EmployeeService)) as EmployeeService;

            // Popular combobox de cargos
            RoleBox.ItemsSource = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();

            // Carregar usuário atual
            LoadCurrentUserAsync();
        }

        private async void LoadCurrentUserAsync()
        {
            try
            {
                // Em um cenário real, você armazenaria o usuário atual em algum lugar (como ApplicationData.Current.LocalSettings)
                // Aqui estamos simulando obter o username do usuário logado 
                var settings = ApplicationData.Current.LocalSettings;
                string currentUsername = settings.Values["CurrentUsername"] as string ?? "admin"; // Valor padrão para teste

                _currentUser = await _authService.GetEmployeeByUsernameAsync(currentUsername);

                // Verificar se o usuário tem permissão para adicionar funcionários
                if (_currentUser != null && !await _authService.HasPermissionAsync(_currentUser, Permissions.AddEmployee))
                {
                    // Desabilitar o diálogo e mostrar mensagem
                    this.IsPrimaryButtonEnabled = false;
                    ValidationInfoBar.Message = "Você não tem permissão para adicionar funcionários";
                    ValidationInfoBar.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                ValidationInfoBar.Message = $"Erro ao carregar dados: {ex.Message}";
                ValidationInfoBar.IsOpen = true;
            }
        }

        private void ValidateInput(object sender, object e)
        {
            bool isValid = !string.IsNullOrEmpty(NameBox.Text) &&
                           !string.IsNullOrEmpty(EmailBox.Text) &&
                           !string.IsNullOrEmpty(PhoneBox.Text) &&
                           !string.IsNullOrEmpty(DocumentBox.Text) &&
                           !string.IsNullOrEmpty(UsernameBox.Text) &&
                           !string.IsNullOrEmpty(PasswordBox.Password) &&
                           RoleBox.SelectedItem != null &&
                           !string.IsNullOrEmpty(PositionBox.Text) &&
                           HireDatePicker.Date != null;

            this.IsPrimaryButtonEnabled = isValid;
            ValidationInfoBar.IsOpen = !isValid;

            if (!isValid)
            {
                ValidationInfoBar.Message = "Preencha todos os campos obrigatórios";
            }
            else
            {
                ValidationInfoBar.IsOpen = false;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Adiar o fechamento do diálogo até concluirmos a operação
            args.Cancel = true;

            try
            {
                // Verificar permissão novamente
                if (_currentUser != null && !await _authService.HasPermissionAsync(_currentUser, Permissions.AddEmployee))
                {
                    ValidationInfoBar.Message = "Você não tem permissão para adicionar funcionários";
                    ValidationInfoBar.IsOpen = true;
                    return;
                }

                // Criar DTO com os dados do formulário
                var dto = new CreateEmployeeDto
                {
                    Name = NameBox.Text,
                    Email = EmailBox.Text,
                    Phone = PhoneBox.Text,
                    Document = DocumentBox.Text,
                    Address = AddressBox.Text,
                    City = CityBox.Text,
                    State = StateBox.Text,
                    ZipCode = ZipCodeBox.Text,
                    BirthDate = BirthDatePicker.Date.Value.DateTime,
                    Username = UsernameBox.Text,
                    Password = PasswordBox.Password,
                    Role = (UserRole)RoleBox.SelectedItem,
                    Position = PositionBox.Text,
                    HireDate = HireDatePicker.Date.Value.DateTime
                };

                // Salvar funcionário
                CreatedEmployee = await _employeeService.CreateEmployeeAsync(_currentUser, dto);

                // Se chegarmos aqui, a operação foi bem-sucedida
                this.Hide();
            }
            catch (UnauthorizedAccessException ex)
            {
                ValidationInfoBar.Message = ex.Message;
                ValidationInfoBar.IsOpen = true;
            }
            catch (InvalidOperationException ex)
            {
                ValidationInfoBar.Message = ex.Message;
                ValidationInfoBar.IsOpen = true;
            }
            catch (Exception ex)
            {
                ValidationInfoBar.Message = $"Erro ao salvar: {ex.Message}";
                ValidationInfoBar.IsOpen = true;
            }
        }

        // Método para editar um funcionário existente
        public void SetEmployeeForEdit(Employee employee)
        {
            if (employee == null) return;

            _isEditMode = true;
            Title = "Editar Funcionário";

            // Preencher campos
            NameBox.Text = employee.Name;
            EmailBox.Text = employee.Email;
            PhoneBox.Text = employee.Phone;
            DocumentBox.Text = employee.Document;
            AddressBox.Text = employee.Address;
            CityBox.Text = employee.City;
            StateBox.Text = employee.State;
            ZipCodeBox.Text = employee.ZipCode;
            BirthDatePicker.Date = employee.BirthDate;
            UsernameBox.Text = employee.Username;
            PasswordBox.Password = "********"; // Nunca mostrar a senha real
            PasswordBox.IsEnabled = false; // Desabilitar campo de senha na edição
            RoleBox.SelectedItem = employee.Role;
            PositionBox.Text = employee.Position;
            HireDatePicker.Date = employee.HireDate;

            // Esconder campos de senha na edição
            PasswordLabel.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Collapsed;
        }
    }
}