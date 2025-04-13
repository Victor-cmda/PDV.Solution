using PDV.Domain.Constants;
using PDV.Domain.Entities;
using PDV.Domain.Enums;

namespace PDV.Domain.Services
{
    /// <summary>
    /// Serviço para gerenciar permissões baseadas em papéis (RBAC)
    /// </summary>
    public static class PermissionService
    {
        // Mapeamento de funções para permissões
        public static Dictionary<UserRole, List<string>> RolePermissions = new Dictionary<UserRole, List<string>>
        {
            // Administrador tem todas as permissões
            {
                UserRole.Admin, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts, Permissions.AddProduct, Permissions.EditProduct, Permissions.DeleteProduct,
                    Permissions.ViewSales, Permissions.AddSale, Permissions.CancelSale, Permissions.ApplyDiscount,
                    Permissions.ViewReports, Permissions.ExportReports,
                    Permissions.ViewEmployees, Permissions.AddEmployee, Permissions.EditEmployee, Permissions.DeleteEmployee,
                    Permissions.OpenCashier, Permissions.CloseCashier, Permissions.CashWithdrawal, Permissions.CashDeposit,
                    Permissions.ViewSettings, Permissions.EditSettings
                }
            },
            // Gerente tem acesso amplo
            {
                UserRole.Manager, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts, Permissions.AddProduct, Permissions.EditProduct,
                    Permissions.ViewSales, Permissions.AddSale, Permissions.CancelSale, Permissions.ApplyDiscount,
                    Permissions.ViewReports, Permissions.ExportReports,
                    Permissions.ViewEmployees, Permissions.AddEmployee, Permissions.EditEmployee,
                    Permissions.OpenCashier, Permissions.CloseCashier, Permissions.CashWithdrawal, Permissions.CashDeposit,
                    Permissions.ViewSettings
                }
            },
            // Caixa tem acesso às funções de caixa
            {
                UserRole.Cashier, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts,
                    Permissions.ViewSales, Permissions.AddSale, Permissions.ApplyDiscount,
                    Permissions.OpenCashier, Permissions.CloseCashier
                }
            },
            // Vendedor tem acesso às funções de venda
            {
                UserRole.Salesperson, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts,
                    Permissions.ViewSales, Permissions.AddSale,
                    Permissions.ViewReports
                }
            },
            // Estoquista tem acesso às funções de estoque
            {
                UserRole.Stockist, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts, Permissions.AddProduct, Permissions.EditProduct,
                    Permissions.ViewReports
                }
            },
            // Viewer só pode visualizar
            {
                UserRole.Viewer, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewProducts,
                    Permissions.ViewSales,
                    Permissions.ViewReports,
                    Permissions.ViewEmployees
                }
            }
        };

        /// <summary>
        /// Gera lista de permissões para um papel específico
        /// </summary>
        public static List<string> GetPermissionsForRole(UserRole role)
        {
            if (RolePermissions.TryGetValue(role, out List<string> permissions))
            {
                return permissions;
            }

            return new List<string>();
        }

        /// <summary>
        /// Verifica se um funcionário possui uma permissão específica
        /// </summary>
        public static bool HasPermission(Employee employee, string permission)
        {
            if (employee == null || !employee.IsActive || employee.IsLocked)
                return false;

            // Administradores têm todas as permissões
            if (employee.Role == UserRole.Admin)
                return true;

            return employee.Permissions.Contains(permission);
        }

        /// <summary>
        /// Atualiza as permissões de um funcionário com base em seu papel
        /// </summary>
        public static void UpdateEmployeePermissions(Employee employee)
        {
            employee.Permissions = GetPermissionsForRole(employee.Role);
        }
    }
}