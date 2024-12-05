using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EmployeeDialog : ContentDialog
    {
        public Employee Employee { get; private set; }
        private bool IsEditMode;

        public EmployeeDialog()
        {
            this.InitializeComponent();
        }

        public EmployeeDialog(Employee employee) : this()
        {
            IsEditMode = true;
            Employee = employee;
            Title = "Editar Funcionário";

            // Preencher campos com dados existentes
            NameBox.Text = employee.Name;
            EmailBox.Text = employee.Email;
            PhoneBox.Text = employee.Phone;
            HireDatePicker.Date = employee.HireDate;

            // Selecionar cargo
            foreach (ComboBoxItem item in RoleBox.Items)
            {
                if (item.Content.ToString() == employee.Role)
                {
                    RoleBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ValidateInput(object sender, object e)
        {
            bool isValid = true;
            string errorMessage = "";

            // Validar Nome
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                isValid = false;
                errorMessage = "Nome é obrigatório";
            }
            else if (NameBox.Text.Length < 3)
            {
                isValid = false;
                errorMessage = "Nome deve ter pelo menos 3 caracteres";
            }

            // Validar Cargo
            if (RoleBox.SelectedItem == null && isValid)
            {
                isValid = false;
                errorMessage = "Cargo é obrigatório";
            }

            // Validar Email
            if (string.IsNullOrWhiteSpace(EmailBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Email é obrigatório";
            }
            else if (!Regex.IsMatch(EmailBox.Text, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$") && isValid)
            {
                isValid = false;
                errorMessage = "Email inválido";
            }

            // Validar Telefone
            if (string.IsNullOrWhiteSpace(PhoneBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Telefone é obrigatório";
            }
            else if (!Regex.IsMatch(PhoneBox.Text, @"^\(\d{2}\) \d{4,5}-\d{4}$") && isValid)
            {
                isValid = false;
                errorMessage = "Telefone inválido. Use o formato (00) 00000-0000";
            }

            // Validar Data de Admissão
            if (HireDatePicker.Date == null && isValid)
            {
                isValid = false;
                errorMessage = "Data de admissão é obrigatória";
            }

            // Atualizar UI
            ValidationInfoBar.Message = errorMessage;
            ValidationInfoBar.IsOpen = !isValid;
            IsPrimaryButtonEnabled = isValid;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Employee = new Employee
            {
                Name = NameBox.Text,
                Role = (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                HireDate = HireDatePicker.Date?.Date ?? DateTime.Now
            };
        }
    }
}
