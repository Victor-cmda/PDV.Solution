using Microsoft.Extensions.Logging;
using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using PDV.Domain.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PDV.Application.Services
{
    /// <summary>
    /// Implementação do serviço de autenticação
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IUnitOfWork unitOfWork, ILogger<AuthenticationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var employee = await GetEmployeeByUsernameAsync(username);
                if (employee == null || !employee.IsActive || employee.IsLocked)
                {
                    return false;
                }

                bool isValid = VerifyPasswordHash(password, employee.PasswordHash, employee.PasswordSalt);

                if (isValid)
                {
                    // Reset failed attempts and update last login
                    employee.FailedLoginAttempts = 0;
                    employee.LastLoginDate = DateTime.UtcNow;
                    await _unitOfWork.CompleteAsync();
                    return true;
                }
                else
                {
                    // Increment failed attempts
                    employee.FailedLoginAttempts++;

                    // Lock account after 5 failed attempts
                    if (employee.FailedLoginAttempts >= 5)
                    {
                        employee.IsLocked = true;
                        _logger.LogWarning($"Account locked for user: {username} after 5 failed attempts");
                    }

                    await _unitOfWork.CompleteAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating credentials for user: {username}");
                return false;
            }
        }

        public async Task<Employee> GetEmployeeByUsernameAsync(string username)
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                return employees.FirstOrDefault(e => e.Username?.ToLower() == username.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting employee by username: {username}");
                return null;
            }
        }

        public async Task<bool> HasPermissionAsync(Employee employee, string permission)
        {
            if (employee == null || !employee.IsActive || employee.IsLocked)
                return false;

            return await Task.FromResult(PermissionService.HasPermission(employee, permission));
        }

        public async Task<(string Hash, string Salt)> GeneratePasswordHashAsync(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                string salt = Convert.ToBase64String(hmac.Key);
                string hash = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

                return await Task.FromResult((hash, salt));
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }

        public async Task<bool> ChangePasswordAsync(Employee employee, string currentPassword, string newPassword)
        {
            if (employee == null)
                return false;

            // Verificar senha atual
            if (!VerifyPasswordHash(currentPassword, employee.PasswordHash, employee.PasswordSalt))
                return false;

            // Gerar nova senha
            var (newHash, newSalt) = await GeneratePasswordHashAsync(newPassword);

            employee.PasswordHash = newHash;
            employee.PasswordSalt = newSalt;

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> LockAccountAsync(string username)
        {
            var employee = await GetEmployeeByUsernameAsync(username);
            if (employee == null)
                return false;

            employee.IsLocked = true;
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UnlockAccountAsync(string username)
        {
            var employee = await GetEmployeeByUsernameAsync(username);
            if (employee == null)
                return false;

            employee.IsLocked = false;
            employee.FailedLoginAttempts = 0;
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string username)
        {
            var employee = await GetEmployeeByUsernameAsync(username);
            if (employee == null)
                return null;

            // Gerar token aleatório
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            // Definir validade (24 horas)
            employee.ResetPasswordToken = token;
            employee.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _unitOfWork.CompleteAsync();
            return token;
        }

        public async Task<bool> ResetPasswordWithTokenAsync(string username, string token, string newPassword)
        {
            var employee = await GetEmployeeByUsernameAsync(username);
            if (employee == null)
                return false;

            // Verificar se token é válido
            if (employee.ResetPasswordToken != token ||
                employee.ResetPasswordTokenExpiry == null ||
                employee.ResetPasswordTokenExpiry < DateTime.UtcNow)
                return false;

            // Gerar nova senha
            var (newHash, newSalt) = await GeneratePasswordHashAsync(newPassword);

            employee.PasswordHash = newHash;
            employee.PasswordSalt = newSalt;
            employee.ResetPasswordToken = null;
            employee.ResetPasswordTokenExpiry = null;
            employee.FailedLoginAttempts = 0;
            employee.IsLocked = false;

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}