using PDV.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PDV.Application.DTOs
{
    /// <summary>
    /// DTO para criação de funcionários
    /// </summary>
    public class CreateEmployeeDto
    {
        // Informações pessoais
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone em formato inválido")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
        public string Document { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        public DateTime BirthDate { get; set; }

        // Dados de acesso
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Nome de usuário deve ter entre 3 e 50 caracteres")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; }

        // Dados funcionais
        [Required(ErrorMessage = "Cargo é obrigatório")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Função é obrigatória")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Data de admissão é obrigatória")]
        public DateTime HireDate { get; set; }

        public string EmployeeCode { get; set; }
    }
}