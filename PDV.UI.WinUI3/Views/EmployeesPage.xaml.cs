using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using PDV.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using System.Windows.Input;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class EmployeesPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Employee> AllEmployees;
        private ObservableCollection<Employee> _filteredEmployees;
        private Employee _selectedEmployee;
        private bool _isLoading;
        private string _sortOption = "NameAsc";
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmployeeService _employeeService;
        private Windows.Foundation.Size _currentWindowSize;
        private DisplayModeEnum _currentDisplayMode = DisplayModeEnum.Expanded;

        // Enum para controlar o modo de exibição
        public enum DisplayModeEnum
        {
            Compact, // Tela pequena
            Medium,  // Tela média
            Expanded // Tela grande
        }

        // Propriedades de controle de layout responsivo
        public DisplayModeEnum CurrentDisplayMode
        {
            get => _currentDisplayMode;
            set
            {
                if (_currentDisplayMode != value)
                {
                    _currentDisplayMode = value;
                    NotifyPropertyChanged(nameof(CurrentDisplayMode));
                    NotifyPropertyChanged(nameof(IsCompactMode));
                    NotifyPropertyChanged(nameof(IsMediumMode));
                    NotifyPropertyChanged(nameof(IsExpandedMode));
                    UpdateLayoutForCurrentDisplayMode();
                }
            }
        }

        public bool IsCompactMode => CurrentDisplayMode == DisplayModeEnum.Compact;
        public bool IsMediumMode => CurrentDisplayMode == DisplayModeEnum.Medium;
        public bool IsExpandedMode => CurrentDisplayMode == DisplayModeEnum.Expanded;

        // Propriedades observáveis
        public ObservableCollection<Employee> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                _filteredEmployees = value;
                NotifyPropertyChanged(nameof(FilteredEmployees));
                NotifyPropertyChanged(nameof(HasItems));
                NotifyPropertyChanged(nameof(HasNoItems));
            }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                NotifyPropertyChanged(nameof(SelectedEmployee));
                NotifyPropertyChanged(nameof(HasSelectedEmployee));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        // Propriedades de estado
        public bool HasItems => FilteredEmployees?.Count > 0;
        public bool HasNoItems => !HasItems;
        public bool HasSelectedEmployee => SelectedEmployee != null;

        // Comando para retornar da visualização de detalhes
        private RelayCommand _navigateBackCommand;
        public RelayCommand NavigateBackCommand => _navigateBackCommand ?? (_navigateBackCommand = new RelayCommand(NavigateBack));

        private void NavigateBack()
        {
            // No modo compacto, voltar da visualização de detalhes para a lista
            if (IsCompactMode && HasSelectedEmployee)
            {
                SelectedEmployee = null;
            }
        }

        // Implementação do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeesPage()
        {
            this.InitializeComponent();

            // Obter serviços
            var app = Microsoft.UI.Xaml.Application.Current as App;
            _unitOfWork = app?.Services.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            _employeeService = app?.Services.GetService(typeof(EmployeeService)) as EmployeeService;

            // Inicializar coleções
            AllEmployees = new ObservableCollection<Employee>();
            FilteredEmployees = new ObservableCollection<Employee>();
            this.DataContext = this;

            // Registrar para eventos de redimensionamento
            this.SizeChanged += EmployeesPage_SizeChanged;

            // Garantir que a inicialização da UI ocorra após a página ser carregada completamente
            this.Loaded += (s, e) =>
            {
                // Configurar os comboboxes
                SetupComboBoxes();
                
                // Carregar dados
                LoadEmployeesAsync();
                
                // Verificar se é a primeira vez que o usuário abre a página
                if (AppSettings.GetValue<bool>("FirstTimeEmployeesPage", true))
                {
                    FeatureTip.IsOpen = true;
                    AppSettings.SetValue("FirstTimeEmployeesPage", false);
                }

                // Inicializar o layout com base no tamanho atual
                _currentWindowSize = new Windows.Foundation.Size(this.ActualWidth, this.ActualHeight);
                UpdateDisplayModeForSize(_currentWindowSize);
            };
        }

        private void EmployeesPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentWindowSize = e.NewSize;
            UpdateDisplayModeForSize(_currentWindowSize);
        }

        private void UpdateDisplayModeForSize(Windows.Foundation.Size size)
        {
            double width = size.Width;
            
            // Thresholds (usando os mesmos valores do ResourceDictionary)
            double compactThreshold = 641;
            double mediumThreshold = 1008;

            if (width < compactThreshold)
            {
                CurrentDisplayMode = DisplayModeEnum.Compact;
            }
            else if (width < mediumThreshold)
            {
                CurrentDisplayMode = DisplayModeEnum.Medium;
            }
            else
            {
                CurrentDisplayMode = DisplayModeEnum.Expanded;
            }
        }

        private void UpdateLayoutForCurrentDisplayMode()
        {
            // Aqui você pode atualizar o layout programaticamente com base no modo de exibição atual
            // Isso complementa as alterações definidas pelo VisualStateManager

            // Como não temos um SplitView disponível, vamos apenas notificar mudanças de propriedades
            NotifyPropertyChanged(nameof(IsCompactMode));
            NotifyPropertyChanged(nameof(IsMediumMode));
            NotifyPropertyChanged(nameof(IsExpandedMode));
        }

        private void SetupComboBoxes()
        {
            // Configurar combobox de ordenação
            if (SortingComboBox != null)
            {
                SortingComboBox.SelectedIndex = 0; // Nome (A-Z) por padrão
            }

            // Configurar combobox de filtro de status
            if (StatusFilterBox != null)
            {
                StatusFilterBox.SelectedIndex = 0; // Todos por padrão
            }

            // Configurar combobox de filtro de cargo
            if (FilterBox != null)
            {
                FilterBox.SelectedIndex = 0; // Todos por padrão
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee updatedEmployee)
            {
                var existingEmployee = AllEmployees.FirstOrDefault(emp => emp.Id == updatedEmployee.Id);

                if (existingEmployee != null)
                {
                    // Atualizar funcionário existente
                    int index = AllEmployees.IndexOf(existingEmployee);
                    AllEmployees[index] = updatedEmployee;
                }
                else
                {
                    // Adicionar novo funcionário
                    AllEmployees.Add(updatedEmployee);
                }

                // Aplicar filtros para atualizar a lista
                ApplyFilters();

                // Selecionar o funcionário atualizado ou adicionado
                EmployeesList.SelectedItem = updatedEmployee;
                SelectedEmployee = updatedEmployee;
            }
            else
            {
                // Recarregar dados ao retornar para a página
                LoadEmployeesAsync();
            }
        }

        private async void LoadEmployeesAsync()
        {
            try
            {
                IsLoading = true;

                // Limpar lista atual
                AllEmployees.Clear();

                // Carregar funcionários do banco de dados
                var employees = await _employeeService.GetAllEmployeesAsync();
                
                // Adicionar à coleção
                foreach (var employee in employees)
                {
                    AllEmployees.Add(employee);
                }

                // Aplicar filtros e ordenação iniciais
                ApplyFilters();

                // Selecionar o primeiro funcionário automaticamente (opcional)
                if (FilteredEmployees.Count > 0)
                {
                    EmployeesList.SelectedIndex = 0;
                    SelectedEmployee = FilteredEmployees[0];
                }
            }
            catch (Exception ex)
            {
                // Gerenciar erro
                await DialogService.ShowErrorAsync("Erro ao carregar funcionários", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para a página de formulário em modo de adição
            Frame.Navigate(typeof(Forms.EmployeeFormPage), null, new DrillInNavigationTransitionInfo());
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(Forms.EmployeeFormPage), SelectedEmployee, new DrillInNavigationTransitionInfo());
            }
        }

        private void QuickEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(Forms.EmployeeFormPage), employee, new DrillInNavigationTransitionInfo());
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                await ShowDeleteConfirmationDialog(SelectedEmployee);
            }
        }

        private async void QuickDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                await ShowDeleteConfirmationDialog(employee);
            }
        }

        private async Task ShowDeleteConfirmationDialog(Employee employee)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Confirmar exclusão",
                Content = $"Tem certeza que deseja excluir o funcionário '{employee.Name}'?",
                PrimaryButtonText = "Excluir",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // Remover funcionário do banco de dados
                    await _unitOfWork.Employees.DeleteAsync(employee);
                    await _unitOfWork.CompleteAsync();
                    
                    // Remover da coleção local
                    AllEmployees.Remove(employee);
                    
                    // Atualizar a lista filtrada
                    ApplyFilters();
                    
                    // Exibir mensagem de sucesso
                    await DialogService.ShowSuccessAsync("Funcionário excluído", 
                        $"O funcionário '{employee.Name}' foi excluído com sucesso.");
                }
                catch (Exception ex)
                {
                    // Exibir mensagem de erro
                    await DialogService.ShowErrorAsync("Erro ao excluir funcionário", 
                        $"Não foi possível excluir o funcionário: {ex.Message}");
                }
            }
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeesAsync();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ApplyFilters();
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ApplyFilters();
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TogglePaneButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void StatusFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _sortOption = selectedItem.Tag?.ToString() ?? "NameAsc";
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            // Verifica se a coleção AllEmployees está inicializada
            if (AllEmployees == null)
            {
                AllEmployees = new ObservableCollection<Employee>();
            }

            // Inicia com todos os funcionários
            var query = AllEmployees.AsQueryable();

            // Aplicar filtro de texto (pesquisa)
            if (SearchBox != null)
            {
                string searchText = SearchBox.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(e =>
                        e.Name.ToLower().Contains(searchText) ||
                        e.Email.ToLower().Contains(searchText) ||
                        e.Phone.ToLower().Contains(searchText) ||
                        e.Position.ToLower().Contains(searchText) ||
                        e.EmployeeCode.ToLower().Contains(searchText)
                    );
                }
            }

            // Aplicar filtro de cargo
            if (FilterBox != null && FilterBox.SelectedItem is ComboBoxItem roleItem && roleItem.Tag != null)
            {
                string roleFilter = roleItem.Tag.ToString();
                if (roleFilter != "All")
                {
                    UserRole selectedRole = Enum.Parse<UserRole>(roleFilter);
                    query = query.Where(e => e.Role == selectedRole);
                }
            }

            // Aplicar filtro de status
            if (StatusFilterBox != null && StatusFilterBox.SelectedItem != null && StatusFilterBox.SelectedItem is ComboBoxItem statusItem && statusItem.Tag != null)
            {
                string statusFilter = statusItem.Tag.ToString();
                switch (statusFilter)
                {
                    case "Active":
                        query = query.Where(e => e.IsActive);
                        break;
                    case "Inactive":
                        query = query.Where(e => !e.IsActive);
                        break;
                    case "Locked":
                        query = query.Where(e => e.IsLocked);
                        break;
                    // Caso "All", não aplicar filtro
                }
            }

            // Aplicar ordenação
            if (SortingComboBox != null && SortingComboBox.SelectedItem != null)
            {
                switch (_sortOption)
                {
                    case "NameAsc":
                        query = query.OrderBy(e => e.Name);
                        break;
                    case "NameDesc":
                        query = query.OrderByDescending(e => e.Name);
                        break;
                    case "HireDateAsc":
                        query = query.OrderBy(e => e.HireDate);
                        break;
                    case "HireDateDesc":
                        query = query.OrderByDescending(e => e.HireDate);
                        break;
                    case "RoleAsc":
                        query = query.OrderBy(e => e.Role).ThenBy(e => e.Name);
                        break;
                    case "RoleDesc":
                        query = query.OrderByDescending(e => e.Role).ThenBy(e => e.Name);
                        break;
                }
            }

            // Atualizar a coleção filtrada
            FilteredEmployees = new ObservableCollection<Employee>(query.ToList());
        }

        private void EmployeesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee employee)
            {
                SelectedEmployee = employee;
            }
        }
    }
}