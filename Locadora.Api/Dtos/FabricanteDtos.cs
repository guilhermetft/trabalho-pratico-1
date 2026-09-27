using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos;

public class FabricanteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PaisOrigem { get; set; } = string.Empty;
}

public class FabricanteCreateDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string PaisOrigem { get; set; } = string.Empty;
}

public class FabricanteUpdateDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string PaisOrigem { get; set; } = string.Empty;
}
