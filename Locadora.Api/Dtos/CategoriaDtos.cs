using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorDiariaBase { get; set; }
}

public class CategoriaCreateDto
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descricao { get; set; }

    [Range(0.01, 100000)]
    public decimal ValorDiariaBase { get; set; }
}

public class CategoriaUpdateDto
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descricao { get; set; }

    [Range(0.01, 100000)]
    public decimal ValorDiariaBase { get; set; }
}
