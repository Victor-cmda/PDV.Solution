using PDV.Domain.Entities;
using System.Threading.Tasks;

namespace PDV.Domain.Interfaces
{
    /// <summary>
    /// Interface para o serviço de autenticação
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Valida as credenciais de um usuário
        /// </summary>
        Task<bool> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// Obtém um funcionário pelo nome de usuário
        /// </summary>
        Task<Employee> GetEmployeeByUsernameAsync(string username);

        /// <summary>
        /// Verifica se um funcionário possui uma permissão específica
        /// </summary>
        Task<bool> HasPermissionAsync(Employee employee, string permission);

        /// <summary>
        /// Gera um hash para uma senha e retorna o hash e o salt
        /// </summary>
        Task<(string Hash, string Salt)> GeneratePasswordHashAsync(string password);

        /// <summary>
        /// Altera a senha de um funcionário
        /// </summary>
        Task<bool> ChangePasswordAsync(Employee employee, string currentPassword, string newPassword);

        /// <summary>
        /// Bloqueia a conta de um funcionário
        /// </summary>
        Task<bool> LockAccountAsync(string username);

        /// <summary>
        /// Desbloqueia a conta de um funcionário
        /// </summary>
        Task<bool> UnlockAccountAsync(string username);

        /// <summary>
        /// Gera um token para redefinição de senha
        /// </summary>
        Task<string> GeneratePasswordResetTokenAsync(string username);

        /// <summary>
        /// Redefine a senha usando um token
        /// </summary>
        Task<bool> ResetPasswordWithTokenAsync(string username, string token, string newPassword);
    }
}