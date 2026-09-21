namespace Locadora.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorDiariaBase { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
