using Locadora.Api.Data;
using Locadora.Api.Dtos;
using Locadora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    private static ClienteDto ToDto(Cliente c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Cpf = c.Cpf,
        Email = c.Email,
        Telefone = c.Telefone,
        Cnh = c.Cnh
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.Nome)
            .Select(c => new ClienteDto { Id = c.Id, Nome = c.Nome, Cpf = c.Cpf, Email = c.Email, Telefone = c.Telefone, Cnh = c.Cnh })
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente {id} não encontrado." });
        }

        return Ok(ToDto(cliente));
    }

    /// <summary>
    /// Filtro 5: clientes que nunca alugaram nenhum veículo.
    /// LEFT JOIN Clientes x Alugueis (GroupJoin + DefaultIfEmpty), diferente do INNER JOIN usado nos demais filtros.
    /// </summary>
    [HttpGet("sem-alugueis")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetSemAlugueis()
    {
        var clientes = await _context.Clientes
            .GroupJoin(
                _context.Alugueis,
                cliente => cliente.Id,
                aluguel => aluguel.ClienteId,
                (cliente, alugueis) => new { cliente, alugueis })
            .SelectMany(
                x => x.alugueis.DefaultIfEmpty(),
                (x, aluguel) => new { x.cliente, aluguel })
            .Where(x => x.aluguel == null)
            .Select(x => new ClienteDto
            {
                Id = x.cliente.Id,
                Nome = x.cliente.Nome,
                Cpf = x.cliente.Cpf,
                Email = x.cliente.Email,
                Telefone = x.cliente.Telefone,
                Cnh = x.cliente.Cnh
            })
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Cpf = dto.Cpf.Trim(),
            Email = dto.Email.Trim(),
            Telefone = dto.Telefone?.Trim(),
            Cnh = dto.Cnh?.Trim()
        };

        _context.Clientes.Add(cliente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um cliente com esse CPF ou e-mail." });
        }

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, ToDto(cliente));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClienteUpdateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente {id} não encontrado." });
        }

        cliente.Nome = dto.Nome.Trim();
        cliente.Cpf = dto.Cpf.Trim();
        cliente.Email = dto.Email.Trim();
        cliente.Telefone = dto.Telefone?.Trim();
        cliente.Cnh = dto.Cnh?.Trim();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um cliente com esse CPF ou e-mail." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente {id} não encontrado." });
        }

        _context.Clientes.Remove(cliente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Cliente possui aluguéis vinculados e não pode ser removido." });
        }

        return NoContent();
    }
}
