namespace PDV.Domain.Enums
{
    /// <summary>
    /// Define os papéis (roles) disponíveis para funcionários no sistema
    /// </summary>
    public enum UserRole
    {
        Admin,          // Acesso total ao sistema
        Manager,        // Acesso amplo, mas não total
        Cashier,        // Operador de caixa
        Salesperson,    // Vendedor
        Stockist,       // Estoquista
        Viewer          // Apenas visualização
    }
}