using Microsoft.Extensions.Logging;
using PDV.Application.DTOs;
using PDV.Domain.Constants;
using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using PDV.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDV.Application.Services
{
    /// <summary>
    /// Serviço para gerenciar funcionários
    /// </summary>
    public class EmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            IUnitOfWork unitOfWork,
            IAuthenticationService authService,
            ILogger<EmployeeService> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo funcionário no sistema
        /// </summary>
        /// <param name="currentUser">Funcionário logado que está criando o novo registro</param>
        /// <param name="dto">Dados do novo funcionário</param>
        /// <returns>O funcionário criado ou null em caso de erro</returns>
        public async Task<Employee> CreateEmployeeAsync(Employee currentUser, CreateEmployeeDto dto)
        {
            try
            {
                // Verificar se o usuário atual tem permissão para criar funcionários
                if (!await _authService.HasPermissionAsync(currentUser, Permissions.AddEmployee))
                {
                    _logger.LogWarning($"Usuário {currentUser.Username} tentou criar um funcionário sem permissão");
                    throw new UnauthorizedAccessException("Você não tem permissão para criar funcionários");
                }

                // Verificar se o nome de usuário já existe
                var existingEmployee = await _authService.GetEmployeeByUsernameAsync(dto.Username);
                if (existingEmployee != null)
                {
                    throw new InvalidOperationException("Este nome de usuário já está em uso");
                }

                // Criar hash da senha
                var (passwordHash, passwordSalt) = await _authService.GeneratePasswordHashAsync(dto.Password);

                // Criar novo funcionário
                var newEmployee = new Employee
                {
                    // Propriedades base de Person
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Document = dto.Document,
                    Address = dto.Address,
                    City = dto.City,
                    State = dto.State,
                    ZipCode = dto.ZipCode,
                    BirthDate = dto.BirthDate,

                    // Propriedades específicas de Employee
                    Username = dto.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = dto.Role,
                    Position = dto.Position,
                    HireDate = dto.HireDate,
                    EmployeeCode = dto.EmployeeCode ?? GenerateEmployeeCode(),
                    ResetPasswordToken = string.Empty,

                    // Valores padrão
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Definir permissões com base no papel
                PermissionService.UpdateEmployeePermissions(newEmployee);

                // Salvar no banco
                await _unitOfWork.Employees.AddAsync(newEmployee);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation($"Funcionário '{newEmployee.Name}' criado por {currentUser.Username}");

                return newEmployee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar funcionário");
                throw;
            }
        }

        /// <summary>
        /// Gera um código único para o funcionário
        /// </summary>
        private string GenerateEmployeeCode()
        {
            return $"EMP{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }

        /// <summary>
        /// Obtém todos os funcionários
        /// </summary>
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _unitOfWork.Employees.GetAllAsync();
        }

        /// <summary>
        /// Obtém funcionário por ID
        /// </summary>
        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _unitOfWork.Employees.GetByIdAsync(id);
        }
    }
}