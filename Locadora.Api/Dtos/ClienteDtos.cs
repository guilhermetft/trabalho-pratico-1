using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cnh { get; set; }
}

public class ClienteCreateDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter apenas dígitos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(11)]
    public string? Cnh { get; set; }
}

public class ClienteUpdateDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter apenas dígitos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(11)]
    public string? Cnh { get; set; }
}
