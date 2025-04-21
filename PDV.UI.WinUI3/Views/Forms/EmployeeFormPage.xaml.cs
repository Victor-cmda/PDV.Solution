using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.UI.WinUI3.Converters;
using PDV.UI.WinUI3.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace PDV.UI.WinUI3.Views.Forms
{
    public sealed partial class EmployeeFormPage : Page, INotifyPropertyChanged
    {
        private Employee _employee;
        private bool _isEditMode;
        private bool _isSaving;
        private bool _hasPermissions;
        private ObservableCollection<PermissionItem> _permissions;

        // Propriedades observ�veis
        public Employee Employee
        {
            get => _employee;
            private set
            {
                _employee = value;
                NotifyPropertyChanged(nameof(Employee));
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                NotifyPropertyChanged(nameof(IsEditMode));
                NotifyPropertyChanged(nameof(PasswordHeaderText));
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                NotifyPropertyChanged(nameof(IsSaving));
            }
        }

        public bool HasPermissions
        {
            get => _hasPermissions;
            set
            {
                _hasPermissions = value;
                NotifyPropertyChanged(nameof(HasPermissions));
            }
        }

        public string PasswordHeaderText => IsEditMode ? "Senha (deixe em branco para manter a atual) *" : "Senha *";

        // Implementa��o do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeeFormPage()
        {
            this.InitializeComponent();

            // Inicializar propriedades
            Employee = new Employee();
            IsEditMode = false;
            IsSaving = false;
            HasPermissions = false;
            _permissions = new ObservableCollection<PermissionItem>();

            // Definir valores padr�o
            SetDefaultValues();

            // Inicializar o ComboBox de cargos
            PopulateRoleComboBox();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee employeeToEdit)
            {
                // Modo de edi��o
                Employee = employeeToEdit;
                IsEditMode = true;

                // Atualizar interface para modo de edi��o
                TitleText.Text = "Editar Funcion�rio";
                ModeText.Text = "Edi��o";
                ModeIcon.Glyph = "\uE70F"; // �cone de edi��o
                ModeBadge.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)); // Azul

                // Preencher os campos com os dados do funcion�rio
                PopulateFormFields();

                // Carregar permiss�es, se dispon�veis
                LoadPermissions();
            }
            else
            {
                // Modo de adi��o
                Employee = new Employee();
                IsEditMode = false;

                // Atualizar interface para modo de adi��o
                TitleText.Text = "Adicionar Funcion�rio";
                ModeText.Text = "Novo Funcion�rio";
                ModeIcon.Glyph = "\uE710"; // �cone de adi��o
                ModeBadge.Background = new SolidColorBrush(Color.FromArgb(255, 0, 140, 0)); // Verde

                // Definir valores padr�o para o novo funcion�rio
                SetDefaultValues();
            }

            // Validar os campos iniciais
            ValidateAllInputs();
        }

        private void SetDefaultValues()
        {
            // Definir valores padr�o para datas
            BirthDatePicker.Date = DateTimeOffset.Now.AddYears(-30);
            HireDatePicker.Date = DateTimeOffset.Now;

            // Definir usu�rio como ativo por padr�o
            UserActiveCheckBox.IsChecked = true;
        }

        private void PopulateRoleComboBox()
        {
            RoleBox.Items.Clear();

            // Adicionar os itens do enum com texto leg�vel
            RoleBox.Items.Add(new ComboBoxItem { Content = "Administrador", Tag = UserRole.Admin });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Gerente", Tag = UserRole.Manager });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Vendedor", Tag = UserRole.Salesperson });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Caixa", Tag = UserRole.Cashier });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Estoquista", Tag = UserRole.Stockist });
        }

        private void PopulateFormFields()
        {
            // Informa��es Pessoais
            NameBox.Text = Employee.Name ?? "";
            EmailBox.Text = Employee.Email ?? "";
            PhoneBox.Text = Employee.Phone ?? "";
            DocumentBox.Text = Employee.Document ?? "";

            if (Employee.BirthDate != default)
                BirthDatePicker.Date = Employee.BirthDate;

            // Endere�o
            AddressBox.Text = Employee.Address ?? "";
            CityBox.Text = Employee.City ?? "";
            StateBox.Text = Employee.State ?? "";
            ZipCodeBox.Text = Employee.ZipCode ?? "";

            // Acesso ao Sistema
            UsernameBox.Text = Employee.Username ?? "";
            PasswordBox.Password = ""; // N�o preencher senha por seguran�a
            UserActiveCheckBox.IsChecked = Employee.IsActive;

            // Dados Funcionais
            SelectRoleByValue(Employee.Role);
            PositionBox.Text = Employee.Position ?? "";
            EmployeeCodeBox.Text = Employee.EmployeeCode ?? "";

            if (Employee.HireDate != default)
                HireDatePicker.Date = Employee.HireDate;

            if (Employee.TerminationDate.HasValue)
                TerminationDatePicker.Date = Employee.TerminationDate.Value;

            // Atualizar status de bloqueio
            if (Employee.IsLocked)
            {
                UnlockUserCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void SelectRoleByValue(UserRole role)
        {
            foreach (ComboBoxItem item in RoleBox.Items)
            {
                if (item.Tag is UserRole itemRole && itemRole == role)
                {
                    RoleBox.SelectedItem = item;
                    UpdateRoleColorIndicator(role);
                    break;
                }
            }
        }

        private void UpdateRoleColorIndicator(UserRole? role)
        {
            if (role.HasValue)
            {
                // Usar o conversor para obter a cor baseada no papel
                var converter = new RoleToColorConverter();
                if (converter.Convert(role.Value, typeof(SolidColorBrush), null, null) is SolidColorBrush brush)
                {
                    RoleColorIndicator.Background = brush;
                    return;
                }
            }

            // Cor padr�o quando nenhum papel est� selecionado
            RoleColorIndicator.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
        }

        private void LoadPermissions()
        {
            // Lista simulada de permiss�es
            var availablePermissions = new List<string>
            {
                "gerenciar_funcionarios",
                "gerenciar_produtos",
                "realizar_vendas",
                "gerenciar_estoque",
                "acessar_relatorios",
                "gerenciar_caixa",
                "configurar_sistema",
                "gerenciar_clientes"
            };

            _permissions.Clear();

            foreach (var permission in availablePermissions)
            {
                // Verificar se o funcion�rio j� possui esta permiss�o
                bool isChecked = Employee.Permissions?.Contains(permission) ?? false;

                // Criar um nome leg�vel para exibi��o
                string displayName = permission.Replace("_", " ").ToUpperInvariant()[0] + permission.Replace("_", " ").Substring(1);

                _permissions.Add(new PermissionItem
                {
                    PermissionKey = permission,
                    DisplayName = displayName,
                    IsChecked = isChecked
                });
            }

            // Atualizar a fonte de dados da lista de permiss�es
            PermissionsList.ItemsSource = _permissions;

            // Atualizar o status de permiss�es
            HasPermissions = _permissions.Count > 0;
        }

        private void ValidateInput(object sender, object e)
        {
            ValidateAllInputs();
        }

        private void ValidateAllInputs()
        {
            // Limpar mensagens de erro anteriores
            ValidationInfoBar.IsOpen = false;
            PersonalInfoValidation.IsOpen = false;
            AccessValidation.IsOpen = false;
            RoleValidation.IsOpen = false;

            bool isPersonalInfoValid = ValidatePersonalInfo();
            bool isAccessValid = ValidateAccess();
            bool isRoleValid = ValidateRole();

            // Habilitar bot�o de salvar apenas se todos os grupos estiverem v�lidos
            SaveButton.IsEnabled = isPersonalInfoValid && isAccessValid && isRoleValid;

            // Exibir mensagem de erro global se necess�rio
            if (!SaveButton.IsEnabled)
            {
                ValidationInfoBar.Message = "Corrija os erros nos campos obrigat�rios para continuar";
                ValidationInfoBar.IsOpen = true;
            }
        }

        private bool ValidatePersonalInfo()
        {
            // Validar campos de informa��es pessoais
            bool isValid = !string.IsNullOrWhiteSpace(NameBox.Text) &&
                           !string.IsNullOrWhiteSpace(EmailBox.Text) &&
                           !string.IsNullOrWhiteSpace(PhoneBox.Text) &&
                           !string.IsNullOrWhiteSpace(DocumentBox.Text) &&
                           BirthDatePicker.Date != null;

            if (!isValid)
            {
                PersonalInfoValidation.Message = "Preencha todos os campos obrigat�rios de informa��es pessoais";
                PersonalInfoValidation.IsOpen = true;
            }

            return isValid;
        }

        private bool ValidateAccess()
        {
            // Validar campos de acesso
            bool isValid = !string.IsNullOrWhiteSpace(UsernameBox.Text) &&
                           (!string.IsNullOrWhiteSpace(PasswordBox.Password) || IsEditMode);

            if (!isValid)
            {
                AccessValidation.Message = IsEditMode
                    ? "O nome de usu�rio � obrigat�rio"
                    : "O nome de usu�rio e a senha s�o obrigat�rios";
                AccessValidation.IsOpen = true;
            }

            return isValid;
        }

        private bool ValidateRole()
        {
            // Validar campos de fun��o
            bool isValid = RoleBox.SelectedItem != null &&
                           !string.IsNullOrWhiteSpace(PositionBox.Text) &&
                           HireDatePicker.Date != null;

            if (!isValid)
            {
                RoleValidation.Message = "Selecione o cargo, informe a fun��o e a data de admiss�o";
                RoleValidation.IsOpen = true;
            }

            return isValid;
        }

        private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoleBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is UserRole role)
            {
                // Atualizar o indicador de cor
                UpdateRoleColorIndicator(role);

                // Atualizar permiss�es com base no cargo, se em modo de edi��o
                if (IsEditMode)
                {
                    // Aqui voc� pode ajustar as permiss�es sugeridas com base no cargo
                    LoadPermissions();
                }
            }
            else
            {
                // Nenhum cargo selecionado, usar cor neutra
                UpdateRoleColorIndicator(null);
            }

            // Validar ap�s a altera��o
            ValidateInput(sender, e);
        }

        private void TerminationDate_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // Se uma data de desligamento foi selecionada, desativar o usu�rio automaticamente
            if (sender.Date.HasValue)
            {
                UserActiveCheckBox.IsChecked = false;
            }

            // Validar ap�s a altera��o
            ValidateInput(sender, null);
        }

        private void SearchByCEP_Click(object sender, RoutedEventArgs e)
        {
            // Implementa��o para buscar o endere�o pelo CEP
            SearchAddressByCEP();
        }

        private async void SearchAddressByCEP()
        {
            string cep = ZipCodeBox.Text?.Trim().Replace("-", "");

            if (string.IsNullOrEmpty(cep) || cep.Length < 8)
            {
                await DialogService.ShowInfoAsync("CEP Inv�lido",
                    "Digite um CEP v�lido para buscar o endere�o.", this.XamlRoot);
                return;
            }

            try
            {
                // Exibir carregamento
                IsSaving = true;

                // Simular uma busca de CEP (em uma implementa��o real, voc� chamaria uma API)
                await Task.Delay(1000);

                // Simular um resultado bem-sucedido
                if (cep == "12345678")
                {
                    AddressBox.Text = "Avenida das Flores";
                    CityBox.Text = "S�o Paulo";
                    StateBox.Text = "SP";
                }
                else
                {
                    // Implementa��o real consultaria uma API de CEP
                    // Para fins de demonstra��o, vamos mostrar um di�logo
                    await DialogService.ShowInfoAsync("Endere�o n�o encontrado",
                        "N�o foi poss�vel encontrar o endere�o para o CEP informado. Verifique se o CEP est� correto.", this.XamlRoot);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync("Erro ao buscar CEP",
                    $"Ocorreu um erro ao buscar o endere�o: {ex.Message}", this.XamlRoot);
            }
            finally
            {
                // Esconder carregamento
                IsSaving = false;
            }
        }

        private void UpdateEmployeeFromForm()
        {
            // Informa��es Pessoais
            Employee.Name = NameBox.Text;
            Employee.Email = EmailBox.Text;
            Employee.Phone = PhoneBox.Text;
            Employee.Document = DocumentBox.Text;

            if (BirthDatePicker.Date.HasValue)
                Employee.BirthDate = BirthDatePicker.Date.Value.DateTime;

            // Endere�o
            Employee.Address = AddressBox.Text;
            Employee.City = CityBox.Text;
            Employee.State = StateBox.Text;
            Employee.ZipCode = ZipCodeBox.Text;

            // Acesso ao Sistema
            Employee.Username = UsernameBox.Text;
            Employee.IsActive = UserActiveCheckBox.IsChecked ?? true;

            // Tratar senha apenas se for nova ou modificada
            if (!IsEditMode || !string.IsNullOrEmpty(PasswordBox.Password))
            {
                // Na implementa��o real, voc� usaria um servi�o de autentica��o
                // para gerar hash e salt da senha
                Employee.PasswordHash = PasswordBox.Password; // Tempor�rio
                Employee.PasswordSalt = "salt"; // Tempor�rio
            }

            // Dados Funcionais
            if (RoleBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is UserRole role)
                Employee.Role = role;

            Employee.Position = PositionBox.Text;

            if (HireDatePicker.Date.HasValue)
                Employee.HireDate = HireDatePicker.Date.Value.DateTime;

            // Data de desligamento (opcional)
            Employee.TerminationDate = TerminationDatePicker.Date?.DateTime;

            // Tratar op��es avan�adas no modo de edi��o
            if (IsEditMode)
            {
                // Desbloquear usu�rio se a op��o estiver marcada
                if (UnlockUserCheckBox.IsChecked == true)
                {
                    Employee.IsLocked = false;
                    Employee.FailedLoginAttempts = 0;
                }

                // For�ar redefini��o de senha
                if (ResetPasswordCheckBox.IsChecked == true)
                {
                    Employee.ResetPasswordToken = Guid.NewGuid().ToString();
                    Employee.ResetPasswordTokenExpiry = DateTime.Now.AddDays(1);
                }

                // Atualizar permiss�es
                if (_permissions.Count > 0)
                {
                    Employee.Permissions = _permissions
                        .Where(p => p.IsChecked)
                        .Select(p => p.PermissionKey)
                        .ToList();
                }
            }

            // Metadados
            if (!IsEditMode)
            {
                Employee.Id = Guid.NewGuid();
                Employee.CreatedAt = DateTime.Now;
                Employee.EmployeeCode = $"EMP{DateTime.Now.ToString("yyyyMMddHHmmss")}";

                // Inicializar lista de permiss�es para novo funcion�rio
                Employee.Permissions = new List<string>();
            }

            Employee.UpdatedAt = DateTime.Now;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                try
                {
                    // Mostrar sobreposi��o de carregamento
                    IsSaving = true;

                    // Atualizar o objeto Employee com os dados do formul�rio
                    UpdateEmployeeFromForm();

                    // Simular um atraso de processamento (para demonstra��o da sobreposi��o de carregamento)
                    await Task.Delay(800);

                    // Navegar de volta para a p�gina de funcion�rios com o resultado
                    if (Frame.CanGoBack)
                    {
                        Frame.Navigate(typeof(EmployeesPage), Employee);
                    }
                }
                catch (Exception ex)
                {
                    // Mostrar mensagem de erro
                    await DialogService.ShowErrorAsync(
                        "Erro ao salvar",
                        $"Ocorreu um erro ao salvar o funcion�rio: {ex.Message}",
                        this.XamlRoot);
                }
                finally
                {
                    // Esconder sobreposi��o de carregamento
                    IsSaving = false;
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

    // Classe para representar um item de permiss�o na lista
    public class PermissionItem : INotifyPropertyChanged
    {
        private string _permissionKey;
        private string _displayName;
        private bool _isChecked;

        public string PermissionKey
        {
            get => _permissionKey;
            set
            {
                _permissionKey = value;
                NotifyPropertyChanged(nameof(PermissionKey));
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged(nameof(DisplayName));
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }

        public override string ToString() => DisplayName;

        // Implementa��o do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}