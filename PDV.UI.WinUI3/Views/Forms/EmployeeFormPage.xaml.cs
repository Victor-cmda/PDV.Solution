using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDV.UI.WinUI3.Views.Forms
{
    public sealed partial class EmployeeFormPage : Page
    {
        public Employee Employee { get; private set; }
        private bool IsEditMode { get; set; }

        public EmployeeFormPage()
        {
            this.InitializeComponent();
            Employee = new Employee();
            IsEditMode = false;

            // Inicializar o ComboBox de cargos com os valores do enum UserRole
            PopulateRoleComboBox();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee employeeToEdit)
            {
                // Modo de edição
                Employee = employeeToEdit;
                IsEditMode = true;
                TitleText.Text = "Editar Funcionário";

                // Preencher os campos com os dados do funcionário
                PopulateFormFields();
            }
            else
            {
                // Modo de adição
                Employee = new Employee();
                IsEditMode = false;
                TitleText.Text = "Adicionar Funcionário";

                // Definir valores padrão
                BirthDatePicker.Date = DateTimeOffset.Now.AddYears(-30);
                HireDatePicker.Date = DateTimeOffset.Now;
            }
        }

        private void PopulateRoleComboBox()
        {
            RoleBox.Items.Clear();
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                RoleBox.Items.Add(new ComboBoxItem { Content = role.ToString() });
            }
        }

        private void PopulateFormFields()
        {
            // Informações Pessoais
            NameBox.Text = Employee.Name;
            EmailBox.Text = Employee.Email;
            PhoneBox.Text = Employee.Phone;
            DocumentBox.Text = Employee.Document;
            BirthDatePicker.Date = Employee.BirthDate;

            // Endereço
            AddressBox.Text = Employee.Address;
            CityBox.Text = Employee.City;
            StateBox.Text = Employee.State;
            ZipCodeBox.Text = Employee.ZipCode;

            // Acesso ao Sistema
            UsernameBox.Text = Employee.Username;
            PasswordBox.Password = ""; // Não preencher senha por segurança

            // Dados Funcionais
            int roleIndex = (int)Employee.Role;
            if (roleIndex >= 0 && roleIndex < RoleBox.Items.Count)
                RoleBox.SelectedIndex = roleIndex;

            PositionBox.Text = Employee.Position;

            if (Employee.HireDate != default)
                HireDatePicker.Date = Employee.HireDate;
        }

        private void ValidateInput(object sender, object e)
        {
            bool isValid = !string.IsNullOrWhiteSpace(NameBox.Text) &&
                           !string.IsNullOrWhiteSpace(EmailBox.Text) &&
                           !string.IsNullOrWhiteSpace(PhoneBox.Text) &&
                           !string.IsNullOrWhiteSpace(DocumentBox.Text) &&
                           BirthDatePicker.Date != null &&
                           !string.IsNullOrWhiteSpace(UsernameBox.Text) &&
                           (!string.IsNullOrWhiteSpace(PasswordBox.Password) || IsEditMode) &&
                           RoleBox.SelectedItem != null &&
                           !string.IsNullOrWhiteSpace(PositionBox.Text) &&
                           HireDatePicker.Date != null;

            SaveButton.IsEnabled = isValid;

            if (!isValid)
            {
                ValidationInfoBar.Message = "Preencha todos os campos obrigatórios (*)";
                ValidationInfoBar.IsOpen = true;
            }
            else
            {
                ValidationInfoBar.IsOpen = false;
            }
        }

        private void UpdateEmployeeFromForm()
        {
            // Informações Pessoais
            Employee.Name = NameBox.Text;
            Employee.Email = EmailBox.Text;
            Employee.Phone = PhoneBox.Text;
            Employee.Document = DocumentBox.Text;
            Employee.BirthDate = BirthDatePicker.Date.Value.DateTime;

            // Endereço
            Employee.Address = AddressBox.Text;
            Employee.City = CityBox.Text;
            Employee.State = StateBox.Text;
            Employee.ZipCode = ZipCodeBox.Text;

            // Acesso ao Sistema
            Employee.Username = UsernameBox.Text;

            // Senha (só atualizar se preenchida em modo de edição)
            if (!IsEditMode || !string.IsNullOrEmpty(PasswordBox.Password))
            {
                // Na implementação real, você usaria o AuthenticationService 
                // para gerar o hash da senha
                Employee.PasswordHash = PasswordBox.Password; // Temporário
                Employee.PasswordSalt = "salt"; // Temporário
            }

            // Dados Funcionais
            if (RoleBox.SelectedIndex >= 0)
                Employee.Role = (UserRole)RoleBox.SelectedIndex;

            Employee.Position = PositionBox.Text;
            Employee.HireDate = HireDatePicker.Date.Value.DateTime;

            // Metadados
            if (!IsEditMode)
            {
                Employee.Id = Guid.NewGuid();
                Employee.CreatedAt = DateTime.Now;
                Employee.IsActive = true;
                Employee.EmployeeCode = $"EMP{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            }

            Employee.UpdatedAt = DateTime.Now;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                UpdateEmployeeFromForm();

                // Navegar de volta para a página de funcionários com o resultado
                if (Frame.CanGoBack)
                {
                    // Retornar resultado através do parâmetro de Navegação
                    Frame.GoBack();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}