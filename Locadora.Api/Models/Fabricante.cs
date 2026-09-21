namespace Locadora.Api.Models;

public class Fabricante
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PaisOrigem { get; set; } = string.Empty;

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
