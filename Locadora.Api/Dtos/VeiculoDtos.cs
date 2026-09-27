using System.ComponentModel.DataAnnotations;
using Locadora.Api.Models;

namespace Locadora.Api.Dtos;

public class VeiculoDto
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public int Quilometragem { get; set; }
    public StatusVeiculo Status { get; set; }

    public int FabricanteId { get; set; }
    public string FabricanteNome { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public decimal ValorDiariaBase { get; set; }
}

public class VeiculoCreateDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Modelo { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", ErrorMessage = "Placa deve seguir o padrão AAA0000 ou AAA0A00 (Mercosul), sem hífen.")]
    public string Placa { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int AnoFabricacao { get; set; }

    [Range(0, int.MaxValue)]
    public int Quilometragem { get; set; }

    [EnumDataType(typeof(StatusVeiculo))]
    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    [Range(1, int.MaxValue, ErrorMessage = "Informe um FabricanteId válido.")]
    public int FabricanteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um CategoriaId válido.")]
    public int CategoriaId { get; set; }
}

public class VeiculoUpdateDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Modelo { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", ErrorMessage = "Placa deve seguir o padrão AAA0000 ou AAA0A00 (Mercosul), sem hífen.")]
    public string Placa { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int AnoFabricacao { get; set; }

    [Range(0, int.MaxValue)]
    public int Quilometragem { get; set; }

    [EnumDataType(typeof(StatusVeiculo))]
    public StatusVeiculo Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um FabricanteId válido.")]
    public int FabricanteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um CategoriaId válido.")]
    public int CategoriaId { get; set; }
}
