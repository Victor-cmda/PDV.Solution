using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Converters;
using PDV.UI.WinUI3.Services;
using PDV.Application.Services;
using PDV.Application.DTOs;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmployeeService _employeeService;
        private readonly IAuthenticationService _authService;
        private Employee _currentUser; // O usuário atual logado no sistema

        // Propriedades observáveis
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

        // Implementação do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeeFormPage()
        {
            this.InitializeComponent();

            // Obter serviços
            var app = Microsoft.UI.Xaml.Application.Current as App;
            _unitOfWork = app?.Services.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            _employeeService = app?.Services.GetService(typeof(EmployeeService)) as EmployeeService;
            _authService = app?.Services.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            
            // Obter usuário atual (mock para este exemplo - em produção usaria um service de sessão)
            // Normalmente seria obtido de um serviço de sessão, mas para simplicidade usaremos o usuário admin
            GetCurrentUserAsync();

            // Inicializar propriedades
            Employee = new Employee();
            IsEditMode = false;
            IsSaving = false;
            HasPermissions = false;
            _permissions = new ObservableCollection<PermissionItem>();

            // Definir valores padrão
            SetDefaultValues();

            // Inicializar o ComboBox de cargos
            PopulateRoleComboBox();
        }

        private async void GetCurrentUserAsync()
        {
            try
            {
                // Em um sistema real, você obteria isso do SessionService
                _currentUser = await _authService.GetEmployeeByUsernameAsync("admin");
                
                if (_currentUser == null)
                {
                    // Fallback para um usuário existente no banco
                    var employees = await _unitOfWork.Employees.GetAllAsync();
                    _currentUser = employees.FirstOrDefault();
                }
            }
            catch
            {
                // Silenciar erros aqui, já que isso é apenas para desenvolvimento
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee employeeToEdit)
            {
                // Modo de edição
                Employee = employeeToEdit;
                IsEditMode = true;

                // Atualizar interface para modo de edição
                TitleText.Text = "Editar Funcionário";
                ModeText.Text = "Edição";
                ModeIcon.Glyph = "\uE70F"; // ícone de edição
                ModeBadge.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)); // Azul

                // Preencher os campos com os dados do funcionário
                PopulateFormFields();

                // Carregar permissões, se disponíveis
                LoadPermissions();
            }
            else
            {
                // Modo de adição
                Employee = new Employee();
                IsEditMode = false;

                // Atualizar interface para modo de adição
                TitleText.Text = "Adicionar Funcionário";
                ModeText.Text = "Novo Funcionário";
                ModeIcon.Glyph = "\uE710"; // ícone de adição
                ModeBadge.Background = new SolidColorBrush(Color.FromArgb(255, 0, 140, 0)); // Verde

                // Definir valores padrão para o novo funcionário
                SetDefaultValues();
            }

            // Validar os campos iniciais
            ValidateAllInputs();
        }

        private void SetDefaultValues()
        {
            // Definir valores padrão para datas
            BirthDatePicker.Date = DateTimeOffset.Now.AddYears(-30);
            HireDatePicker.Date = DateTimeOffset.Now;
            
            // Configurar formato de data
            BirthDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";
            HireDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";
            TerminationDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";

            // Definir usuário como ativo por padrão
            UserActiveCheckBox.IsChecked = true;
        }

        private void PopulateRoleComboBox()
        {
            RoleBox.Items.Clear();

            // Adicionar os itens do enum com texto legível
            RoleBox.Items.Add(new ComboBoxItem { Content = "Administrador", Tag = UserRole.Admin });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Gerente", Tag = UserRole.Manager });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Vendedor", Tag = UserRole.Salesperson });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Caixa", Tag = UserRole.Cashier });
            RoleBox.Items.Add(new ComboBoxItem { Content = "Estoquista", Tag = UserRole.Stockist });
        }

        private void PopulateFormFields()
        {
            // Setup date formats 
            BirthDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";
            HireDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";
            TerminationDatePicker.DateFormat = "{day.integer}/{month.integer}/{year.full}";
            
            // Informações Pessoais
            NameBox.Text = Employee.Name ?? "";
            EmailBox.Text = Employee.Email ?? "";
            PhoneBox.Text = Employee.Phone ?? "";
            DocumentBox.Text = Employee.Document ?? "";

            if (Employee.BirthDate != default)
                BirthDatePicker.Date = Employee.BirthDate;

            // Endereço
            AddressBox.Text = Employee.Address ?? "";
            CityBox.Text = Employee.City ?? "";
            StateBox.Text = Employee.State ?? "";
            ZipCodeBox.Text = Employee.ZipCode ?? "";

            // Acesso ao Sistema
            UsernameBox.Text = Employee.Username ?? "";
            PasswordBox.Password = ""; // Não preencher senha por segurança
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

            // Cor padrão quando nenhum papel está selecionado
            RoleColorIndicator.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
        }

        private async void LoadPermissions()
        {
            try
            {
                // Carregar permissões existentes do funcionário atual
                _permissions.Clear();

                // Lista de todas as permissões disponíveis
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

                foreach (var permission in availablePermissions)
                {
                    // Verificar se o funcionário já possui esta permissão
                    bool isChecked = Employee.Permissions?.Contains(permission) ?? false;

                    // Criar um nome legível para exibição
                    string displayName = permission.Replace("_", " ");
                    displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);

                    _permissions.Add(new PermissionItem
                    {
                        PermissionKey = permission,
                        DisplayName = displayName,
                        IsChecked = isChecked
                    });
                }

                // Atualizar a fonte de dados da lista de permissões
                PermissionsList.ItemsSource = _permissions;

                // Atualizar o status de permissões
                HasPermissions = _permissions.Count > 0;
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync("Erro ao carregar permissões", ex.Message, this.XamlRoot);
            }
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

            // Habilitar botão de salvar apenas se todos os grupos estiverem válidos
            SaveButton.IsEnabled = isPersonalInfoValid && isAccessValid && isRoleValid;

            // Exibir mensagem de erro global se necessário
            if (!SaveButton.IsEnabled)
            {
                ValidationInfoBar.Message = "Corrija os erros nos campos obrigatórios para continuar";
                ValidationInfoBar.IsOpen = true;
            }
        }

        private bool ValidatePersonalInfo()
        {
            // Validar campos de informações pessoais
            bool isValid = !string.IsNullOrWhiteSpace(NameBox.Text) &&
                           !string.IsNullOrWhiteSpace(EmailBox.Text) &&
                           !string.IsNullOrWhiteSpace(PhoneBox.Text) &&
                           !string.IsNullOrWhiteSpace(DocumentBox.Text) &&
                           BirthDatePicker.Date != null;

            if (!isValid)
            {
                PersonalInfoValidation.Message = "Preencha todos os campos obrigatórios de informações pessoais";
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
                    ? "O nome de usuário é obrigatório"
                    : "O nome de usuário e a senha são obrigatórios";
                AccessValidation.IsOpen = true;
            }

            return isValid;
        }

        private bool ValidateRole()
        {
            // Validar campos de função
            bool isValid = RoleBox.SelectedItem != null &&
                           !string.IsNullOrWhiteSpace(PositionBox.Text) &&
                           HireDatePicker.Date != null;

            if (!isValid)
            {
                RoleValidation.Message = "Selecione o cargo, informe a função e a data de admissão";
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

                // Atualizar permissões com base no cargo, se em modo de edição
                if (IsEditMode)
                {
                    // Aqui você pode ajustar as permissões sugeridas com base no cargo
                    LoadPermissions();
                }
            }
            else
            {
                // Nenhum cargo selecionado, usar cor neutra
                UpdateRoleColorIndicator(null);
            }

            // Validar após a alteração
            ValidateInput(sender, e);
        }

        private void TerminationDate_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // Se uma data de desligamento foi selecionada, desativar o usuário automaticamente
            if (sender.Date.HasValue)
            {
                UserActiveCheckBox.IsChecked = false;
            }

            // Validar após a alteração
            ValidateInput(sender, null);
        }

        private void SearchByCEP_Click(object sender, RoutedEventArgs e)
        {
            // Implementação para buscar o endereço pelo CEP
            SearchAddressByCEP();
        }

        private async void SearchAddressByCEP()
        {
            string cep = ZipCodeBox.Text?.Trim().Replace("-", "");

            if (string.IsNullOrEmpty(cep) || cep.Length < 8)
            {
                await DialogService.ShowInfoAsync("CEP Inválido",
                    "Digite um CEP válido para buscar o endereço.", this.XamlRoot);
                return;
            }

            try
            {
                // Exibir carregamento
                IsSaving = true;

                // Simular uma busca de CEP (em uma implementação real, você chamaria uma API)
                await Task.Delay(1000);

                // Simular um resultado bem-sucedido
                if (cep == "12345678")
                {
                    AddressBox.Text = "Avenida das Flores";
                    CityBox.Text = "São Paulo";
                    StateBox.Text = "SP";
                }
                else
                {
                    // Implementação real consultaria uma API de CEP
                    // Para fins de demonstração, vamos mostrar um diálogo
                    await DialogService.ShowInfoAsync("Endereço não encontrado",
                        "Não foi possível encontrar o endereço para o CEP informado. Verifique se o CEP está correto.", this.XamlRoot);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync("Erro ao buscar CEP",
                    $"Ocorreu um erro ao buscar o endereço: {ex.Message}", this.XamlRoot);
            }
            finally
            {
                // Esconder carregamento
                IsSaving = false;
            }
        }

        private CreateEmployeeDto GetEmployeeDataFromForm()
        {
            var dto = new CreateEmployeeDto
            {
                // Propriedades base
                Name = NameBox.Text,
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                Document = DocumentBox.Text,
                Address = AddressBox.Text,
                City = CityBox.Text,
                State = StateBox.Text,
                ZipCode = ZipCodeBox.Text,
                
                // Data de nascimento
                BirthDate = BirthDatePicker.Date.HasValue 
                    ? BirthDatePicker.Date.Value.DateTime 
                    : DateTime.Now.AddYears(-30),

                // Dados de acesso
                Username = UsernameBox.Text,
                Password = PasswordBox.Password,

                // Dados funcionais
                Role = RoleBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is UserRole role
                    ? role : UserRole.Salesperson,
                Position = PositionBox.Text,
                
                // Data de admissão
                HireDate = HireDatePicker.Date.HasValue
                    ? HireDatePicker.Date.Value.DateTime
                    : DateTime.Now,
                
                // Código do funcionário (gerado automaticamente se for novo funcionário)
                EmployeeCode = IsEditMode ? Employee.EmployeeCode : null
            };

            return dto;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                try
                {
                    // Mostrar sobreposição de carregamento
                    IsSaving = true;

                    Employee result;

                    if (IsEditMode)
                    {
                        // Atualizar funcionário existente
                        // Atualizar o objeto Employee com os dados do formulário
                        UpdateEmployeeFromForm();

                        // Aplicar alterações no banco de dados
                        await _unitOfWork.Employees.UpdateAsync(Employee);
                        await _unitOfWork.CompleteAsync();
                        result = Employee;
                    }
                    else
                    {
                        // Criar novo funcionário
                        var dto = GetEmployeeDataFromForm();
                        
                        // Verificar se o usuário atual existe
                        if (_currentUser == null)
                        {
                            // Em cenário de teste, criar com o primeiro usuário disponível
                            var users = await _unitOfWork.Employees.GetAllAsync();
                            _currentUser = users.FirstOrDefault();
                            
                            if (_currentUser == null)
                            {
                                throw new InvalidOperationException("Não foi possível identificar um usuário para criar o funcionário");
                            }
                        }
                        
                        // Chamar o serviço para criar o funcionário
                        result = await _employeeService.CreateEmployeeAsync(_currentUser, dto);
                    }

                    // Navegar de volta para a página de funcionários com o resultado
                    if (Frame.CanGoBack)
                    {
                        Frame.Navigate(typeof(EmployeesPage), result);
                    }
                }
                catch (Exception ex)
                {
                    // Mostrar mensagem de erro
                    await DialogService.ShowErrorAsync(
                        "Erro ao salvar",
                        $"Ocorreu um erro ao salvar o funcionário: {ex.Message}",
                        this.XamlRoot);
                }
                finally
                {
                    // Esconder sobreposição de carregamento
                    IsSaving = false;
                }
            }
        }

        private void UpdateEmployeeFromForm()
        {
            // Informações Pessoais
            Employee.Name = NameBox.Text;
            Employee.Email = EmailBox.Text;
            Employee.Phone = PhoneBox.Text;
            Employee.Document = DocumentBox.Text;

            if (BirthDatePicker.Date.HasValue)
                Employee.BirthDate = BirthDatePicker.Date.Value.DateTime;

            // Endereço
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
                // Na implementação real, você usaria um serviço de autenticação
                // para gerar hash e salt da senha
                // Para este exemplo, vamos atualizar diretamente:
                var task = _authService.GeneratePasswordHashAsync(PasswordBox.Password);
                task.Wait();
                var (hash, salt) = task.Result;
                
                Employee.PasswordHash = hash;
                Employee.PasswordSalt = salt;
            }

            // Dados Funcionais
            if (RoleBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is UserRole role)
                Employee.Role = role;

            Employee.Position = PositionBox.Text;

            if (HireDatePicker.Date.HasValue)
                Employee.HireDate = HireDatePicker.Date.Value.DateTime;

            // Data de desligamento (opcional)
            Employee.TerminationDate = TerminationDatePicker.Date?.DateTime;

            // Tratar opções avançadas no modo de edição
            if (IsEditMode)
            {
                // Desbloquear usuário se a opção estiver marcada
                if (UnlockUserCheckBox.IsChecked == true)
                {
                    Employee.IsLocked = false;
                    Employee.FailedLoginAttempts = 0;
                }

                // Forçar redefinição de senha
                if (ResetPasswordCheckBox.IsChecked == true)
                {
                    Employee.ResetPasswordToken = Guid.NewGuid().ToString();
                    Employee.ResetPasswordTokenExpiry = DateTime.Now.AddDays(1);
                }
                else if (Employee.ResetPasswordToken == null)
                {
                    // Garantir que ResetPasswordToken nunca seja nulo
                    Employee.ResetPasswordToken = string.Empty;
                }

                // Atualizar permissões
                if (_permissions.Count > 0)
                {
                    Employee.Permissions = _permissions
                        .Where(p => p.IsChecked)
                        .Select(p => p.PermissionKey)
                        .ToList();
                }
            }

            // Atualizar timestamp
            Employee.UpdatedAt = DateTime.Now;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }

    // Classe para representar um item de permissão na lista
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

        // Implementação do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}