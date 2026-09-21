namespace Locadora.Api.Models;

public class Aluguel
{
    public int Id { get; set; }

    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }

    // Preenchida somente quando o veículo é devolvido.
    public DateTime? DataDevolucao { get; set; }

    public int KmInicial { get; set; }
    public int? KmFinal { get; set; }

    public decimal ValorDiaria { get; set; }
    public decimal? ValorTotal { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; } = null!;
}
