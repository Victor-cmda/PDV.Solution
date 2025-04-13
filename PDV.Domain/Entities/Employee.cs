using PDV.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PDV.Domain.Entities
{
    /// <summary>
    /// Representa um funcionário no sistema PDV
    /// </summary>
    public class Employee : Person
    {
        // Dados de Autenticação
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }

        // Dados Funcionais
        public UserRole Role { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string EmployeeCode { get; set; } // Código do funcionário para identificação

        // Segurança e Permissões
        public List<string> Permissions { get; set; } = new List<string>();
        public DateTime? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;

        // Tokens para recuperação/renovação
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
    }
}