using Locadora.Api.Data;
using Locadora.Api.Dtos;
using Locadora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class AlugueisController : ControllerBase
{
    private readonly AppDbContext _context;

    public AlugueisController(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<AluguelDto> Projetar(IQueryable<Aluguel> query) => query
        .Select(a => new AluguelDto
        {
            Id = a.Id,
            DataRetirada = a.DataRetirada,
            DataPrevistaDevolucao = a.DataPrevistaDevolucao,
            DataDevolucao = a.DataDevolucao,
            KmInicial = a.KmInicial,
            KmFinal = a.KmFinal,
            ValorDiaria = a.ValorDiaria,
            ValorTotal = a.ValorTotal,
            ClienteId = a.ClienteId,
            ClienteNome = a.Cliente.Nome,
            VeiculoId = a.VeiculoId,
            VeiculoModelo = a.Veiculo.Modelo,
            VeiculoPlaca = a.Veiculo.Placa
        });

    /// <summary>
    /// Lista todos os alugueis.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AluguelDto>>> GetAll()
    {
        var alugueis = await Projetar(_context.Alugueis.AsQueryable())
            .OrderByDescending(a => a.DataRetirada)
            .ToListAsync();

        return Ok(alugueis);
    }

    /// <summary>
    /// Obtém o aluguel pelo Id.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AluguelDto>> GetById(int id)
    {
        var aluguel = await Projetar(_context.Alugueis.Where(a => a.Id == id)).FirstOrDefaultAsync();
        if (aluguel is null)
        {
            return NotFound(new { mensagem = $"Aluguel {id} não encontrado." });
        }

        return Ok(aluguel);
    }

    /// <summary>
    /// Filtro 3: aluguéis por cliente, com dados do veículo.
    /// INNER JOIN Alugueis x Clientes x Veiculos (via LINQ join explícito).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("por-cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<AluguelDto>>> GetPorCliente(int clienteId)
    {
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == clienteId);
        if (!clienteExiste)
        {
            return NotFound(new { mensagem = $"Cliente {clienteId} não encontrado." });
        }

        var alugueis = await (
            from a in _context.Alugueis
            join c in _context.Clientes on a.ClienteId equals c.Id
            join v in _context.Veiculos on a.VeiculoId equals v.Id
            where c.Id == clienteId
            orderby a.DataRetirada descending
            select new AluguelDto
            {
                Id = a.Id,
                DataRetirada = a.DataRetirada,
                DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                DataDevolucao = a.DataDevolucao,
                KmInicial = a.KmInicial,
                KmFinal = a.KmFinal,
                ValorDiaria = a.ValorDiaria,
                ValorTotal = a.ValorTotal,
                ClienteId = c.Id,
                ClienteNome = c.Nome,
                VeiculoId = v.Id,
                VeiculoModelo = v.Modelo,
                VeiculoPlaca = v.Placa
            }).ToListAsync();

        return Ok(alugueis);
    }

    /// <summary>
    /// Filtro 4: aluguéis em atraso (não devolvidos e com previsão de devolução vencida).
    /// INNER JOIN Alugueis x Clientes x Veiculos (via LINQ join explícito).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("atrasados")]
    public async Task<ActionResult<IEnumerable<AluguelDto>>> GetAtrasados()
    {
        var agora = DateTime.Now;

        var alugueis = await (
            from a in _context.Alugueis
            join c in _context.Clientes on a.ClienteId equals c.Id
            join v in _context.Veiculos on a.VeiculoId equals v.Id
            where a.DataDevolucao == null && a.DataPrevistaDevolucao < agora
            orderby a.DataPrevistaDevolucao
            select new AluguelDto
            {
                Id = a.Id,
                DataRetirada = a.DataRetirada,
                DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                DataDevolucao = a.DataDevolucao,
                KmInicial = a.KmInicial,
                KmFinal = a.KmFinal,
                ValorDiaria = a.ValorDiaria,
                ValorTotal = a.ValorTotal,
                ClienteId = c.Id,
                ClienteNome = c.Nome,
                VeiculoId = v.Id,
                VeiculoModelo = v.Modelo,
                VeiculoPlaca = v.Placa
            }).ToListAsync();

        return Ok(alugueis);
    }

    /// <summary>
    /// Cadastra o aluguel.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<ActionResult<AluguelDto>> Create(AluguelCreateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
        {
            return BadRequest(new { mensagem = $"Cliente {dto.ClienteId} não encontrado." });
        }

        var veiculo = await _context.Veiculos.FindAsync(dto.VeiculoId);
        if (veiculo is null)
        {
            return BadRequest(new { mensagem = $"Veículo {dto.VeiculoId} não encontrado." });
        }

        if (veiculo.Status != StatusVeiculo.Disponivel)
        {
            return Conflict(new { mensagem = $"Veículo {veiculo.Placa} não está disponível para locação (status atual: {veiculo.Status})." });
        }

        var aluguel = new Aluguel
        {
            ClienteId = dto.ClienteId,
            VeiculoId = dto.VeiculoId,
            DataRetirada = dto.DataRetirada,
            DataPrevistaDevolucao = dto.DataPrevistaDevolucao,
            ValorDiaria = dto.ValorDiaria,
            KmInicial = veiculo.Quilometragem
        };

        veiculo.Status = StatusVeiculo.Alugado;

        _context.Alugueis.Add(aluguel);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensagem = "Não foi possível registrar o aluguel.", detalhe = ex.InnerException?.Message ?? ex.Message });
        }

        var criado = await Projetar(_context.Alugueis.Where(a => a.Id == aluguel.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = aluguel.Id }, criado);
    }

    /// <summary>
    /// Atualiza o aluguel existente.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AluguelUpdateDto dto)
    {
        var aluguel = await _context.Alugueis.FindAsync(id);
        if (aluguel is null)
        {
            return NotFound(new { mensagem = $"Aluguel {id} não encontrado." });
        }

        if (aluguel.DataDevolucao is not null)
        {
            return Conflict(new { mensagem = "Aluguel já devolvido não pode ser alterado." });
        }

        aluguel.DataRetirada = dto.DataRetirada;
        aluguel.DataPrevistaDevolucao = dto.DataPrevistaDevolucao;
        aluguel.ValorDiaria = dto.ValorDiaria;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensagem = "Não foi possível atualizar o aluguel.", detalhe = ex.InnerException?.Message ?? ex.Message });
        }

        return NoContent();
    }

    /// <summary>
    /// Registra a devolução do veículo: calcula o valor total, atualiza a quilometragem
    /// e libera o veículo (status volta a Disponivel).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("{id:int}/devolucao")]
    public async Task<ActionResult<AluguelDto>> Devolver(int id, AluguelDevolucaoDto dto)
    {
        var aluguel = await _context.Alugueis.Include(a => a.Veiculo).FirstOrDefaultAsync(a => a.Id == id);
        if (aluguel is null)
        {
            return NotFound(new { mensagem = $"Aluguel {id} não encontrado." });
        }

        if (aluguel.DataDevolucao is not null)
        {
            return Conflict(new { mensagem = "Aluguel já foi devolvido." });
        }

        var dataDevolucao = dto.DataDevolucao ?? DateTime.Now;

        if (dataDevolucao < aluguel.DataRetirada)
        {
            return BadRequest(new { mensagem = "DataDevolucao não pode ser anterior à DataRetirada." });
        }

        if (dto.KmFinal < aluguel.KmInicial)
        {
            return BadRequest(new { mensagem = $"KmFinal ({dto.KmFinal}) não pode ser menor que KmInicial ({aluguel.KmInicial})." });
        }

        var dias = Math.Max(1, (int)Math.Ceiling((dataDevolucao - aluguel.DataRetirada).TotalDays));

        aluguel.DataDevolucao = dataDevolucao;
        aluguel.KmFinal = dto.KmFinal;
        aluguel.ValorTotal = dias * aluguel.ValorDiaria;

        aluguel.Veiculo.Quilometragem = dto.KmFinal;
        aluguel.Veiculo.Status = StatusVeiculo.Disponivel;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensagem = "Não foi possível registrar a devolução.", detalhe = ex.InnerException?.Message ?? ex.Message });
        }

        var atualizado = await Projetar(_context.Alugueis.Where(a => a.Id == id)).FirstAsync();
        return Ok(atualizado);
    }

    /// <summary>
    /// Remove o aluguel.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var aluguel = await _context.Alugueis.Include(a => a.Veiculo).FirstOrDefaultAsync(a => a.Id == id);
        if (aluguel is null)
        {
            return NotFound(new { mensagem = $"Aluguel {id} não encontrado." });
        }

        if (aluguel.DataDevolucao is null)
        {
            aluguel.Veiculo.Status = StatusVeiculo.Disponivel;
        }

        _context.Alugueis.Remove(aluguel);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensagem = "Não foi possível remover o aluguel.", detalhe = ex.InnerException?.Message ?? ex.Message });
        }

        return NoContent();
    }
}
