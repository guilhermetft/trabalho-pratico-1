using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos;

public class AluguelDto
{
    public int Id { get; set; }

    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }

    public int KmInicial { get; set; }
    public int? KmFinal { get; set; }

    public decimal ValorDiaria { get; set; }
    public decimal? ValorTotal { get; set; }

    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;

    public int VeiculoId { get; set; }
    public string VeiculoModelo { get; set; } = string.Empty;
    public string VeiculoPlaca { get; set; } = string.Empty;
}

public class AluguelCreateDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe um ClienteId válido.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um VeiculoId válido.")]
    public int VeiculoId { get; set; }

    [Required]
    public DateTime DataRetirada { get; set; }

    [Required]
    public DateTime DataPrevistaDevolucao { get; set; }

    [Range(0.01, 100000)]
    public decimal ValorDiaria { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataPrevistaDevolucao < DataRetirada)
        {
            yield return new ValidationResult(
                "DataPrevistaDevolucao deve ser maior ou igual a DataRetirada.",
                new[] { nameof(DataPrevistaDevolucao) });
        }
    }
}

public class AluguelUpdateDto : IValidatableObject
{
    [Required]
    public DateTime DataRetirada { get; set; }

    [Required]
    public DateTime DataPrevistaDevolucao { get; set; }

    [Range(0.01, 100000)]
    public decimal ValorDiaria { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataPrevistaDevolucao < DataRetirada)
        {
            yield return new ValidationResult(
                "DataPrevistaDevolucao deve ser maior ou igual a DataRetirada.",
                new[] { nameof(DataPrevistaDevolucao) });
        }
    }
}

public class AluguelDevolucaoDto
{
    public DateTime? DataDevolucao { get; set; }

    [Range(0, int.MaxValue)]
    public int KmFinal { get; set; }
}
