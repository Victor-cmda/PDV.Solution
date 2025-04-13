using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.Domain.Interfaces;
using PDV.Domain.Services;
using PDV.Infrastructure.Data.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PDV.Infrastructure.Data
{
    /// <summary>
    /// Serviço para inicializar o banco de dados com dados iniciais
    /// </summary>
    public class SeedService
    {
        private readonly IDbContextFactory<LocalDbContext> _contextFactory;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<SeedService> _logger;

        public SeedService(
            IDbContextFactory<LocalDbContext> contextFactory,
            IAuthenticationService authService,
            ILogger<SeedService> logger)
        {
            _contextFactory = contextFactory;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Inicializa o banco de dados com dados iniciais
        /// </summary>
        public async Task SeedDatabaseAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Verificar se já existe o usuário desenvolvedor
                var devUserExists = await context.Employees
                    .AnyAsync(e => e.Username == "developer");

                if (!devUserExists)
                {
                    await CreateDeveloperUserAsync(context);
                }

                // Verifica se já existe o usuário admin
                var adminUserExists = await context.Employees
                    .AnyAsync(e => e.Username == "admin");

                if (!adminUserExists)
                {
                    await CreateAdminUserAsync(context);
                }

                _logger.LogInformation("Inicialização do banco de dados concluída com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar o banco de dados.");
            }
        }

        /// <summary>
        /// Cria usuário desenvolvedor com acesso total
        /// </summary>
        private async Task CreateDeveloperUserAsync(LocalDbContext context)
        {
            try
            {
                // Gerar senha segura
                var (passwordHash, passwordSalt) = await _authService.GeneratePasswordHashAsync("dev@123");

                var developer = new Employee
                {
                    Name = "Desenvolvedor",
                    Email = "dev@pdvsystem.com",
                    Phone = "(00) 00000-0000",
                    Username = "developer",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = UserRole.Admin,
                    Position = "Desenvolvedor",
                    HireDate = DateTime.Now,
                    BirthDate = new DateTime(1990, 1, 1),
                    EmployeeCode = "DEV001",
                    IsActive = true,
                    ResetPasswordToken = "", // Initialize with empty string to satisfy NOT NULL constraint
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Document = "00000000000", // CPF placeholder
                    Address = "",
                    City = "",
                    State = "",
                    ZipCode = "",
                    DeletedAt = DateTime.MinValue, // Initialize with default value
                    Id = Guid.NewGuid() // Generate new ID
                };

                // Atribuir todas as permissões disponíveis
                PermissionService.UpdateEmployeePermissions(developer);

                // Salvar no banco
                await context.Employees.AddAsync(developer);
                await context.SaveChangesAsync();

                _logger.LogInformation("Usuário desenvolvedor criado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário desenvolvedor.");
                throw;
            }
        }

        /// <summary>
        /// Cria usuário administrador
        /// </summary>
        private async Task CreateAdminUserAsync(LocalDbContext context)
        {
            try
            {
                // Gerar senha segura
                var (passwordHash, passwordSalt) = await _authService.GeneratePasswordHashAsync("admin@123");

                var admin = new Employee
                {
                    Name = "Administrador",
                    Email = "admin@pdvsystem.com",
                    Phone = "(00) 00000-0000",
                    Username = "admin",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = UserRole.Admin,
                    Position = "Administrador",
                    HireDate = DateTime.Now,
                    BirthDate = new DateTime(1990, 1, 1),
                    EmployeeCode = "ADM001",
                    IsActive = true,
                    ResetPasswordToken = "", // Initialize with empty string to satisfy NOT NULL constraint
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Document = "11111111111", // CPF placeholder
                    Address = "",
                    City = "",
                    State = "",
                    ZipCode = "",
                    DeletedAt = DateTime.MinValue, // Initialize with default value
                    Id = Guid.NewGuid() // Generate new ID
                };

                // Atribuir permissões de administrador
                PermissionService.UpdateEmployeePermissions(admin);

                // Salvar no banco
                await context.Employees.AddAsync(admin);
                await context.SaveChangesAsync();

                _logger.LogInformation("Usuário administrador criado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário administrador.");
                throw;
            }
        }
    }
}