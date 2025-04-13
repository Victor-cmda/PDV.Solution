namespace PDV.Domain.Constants
{
    /// <summary>
    /// Define todas as permissões disponíveis no sistema PDV
    /// </summary>
    public static class Permissions
    {
        // Permissões do sistema
        public const string ViewDashboard = "ViewDashboard";

        // Permissões de produtos
        public const string ViewProducts = "ViewProducts";
        public const string AddProduct = "AddProduct";
        public const string EditProduct = "EditProduct";
        public const string DeleteProduct = "DeleteProduct";

        // Permissões de vendas
        public const string ViewSales = "ViewSales";
        public const string AddSale = "AddSale";
        public const string CancelSale = "CancelSale";
        public const string ApplyDiscount = "ApplyDiscount";

        // Permissões de relatórios
        public const string ViewReports = "ViewReports";
        public const string ExportReports = "ExportReports";

        // Permissões de funcionários
        public const string ViewEmployees = "ViewEmployees";
        public const string AddEmployee = "AddEmployee";
        public const string EditEmployee = "EditEmployee";
        public const string DeleteEmployee = "DeleteEmployee";

        // Permissões de caixa
        public const string OpenCashier = "OpenCashier";
        public const string CloseCashier = "CloseCashier";
        public const string CashWithdrawal = "CashWithdrawal";
        public const string CashDeposit = "CashDeposit";

        // Permissões de configuração
        public const string ViewSettings = "ViewSettings";
        public const string EditSettings = "EditSettings";
    }
}