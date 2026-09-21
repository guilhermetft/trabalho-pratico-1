namespace Locadora.Api.Models;

public class Veiculo
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public int Quilometragem { get; set; }
    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    public int FabricanteId { get; set; }
    public Fabricante Fabricante { get; set; } = null!;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
