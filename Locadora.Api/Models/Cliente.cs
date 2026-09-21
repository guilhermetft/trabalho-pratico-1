namespace Locadora.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cnh { get; set; }

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
