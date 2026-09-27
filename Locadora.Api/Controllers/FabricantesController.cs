using Locadora.Api.Data;
using Locadora.Api.Dtos;
using Locadora.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FabricantesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FabricantesController(AppDbContext context)
    {
        _context = context;
    }

    private static FabricanteDto ToDto(Fabricante f) => new()
    {
        Id = f.Id,
        Nome = f.Nome,
        PaisOrigem = f.PaisOrigem
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FabricanteDto>>> GetAll()
    {
        var fabricantes = await _context.Fabricantes
            .OrderBy(f => f.Nome)
            .Select(f => new FabricanteDto { Id = f.Id, Nome = f.Nome, PaisOrigem = f.PaisOrigem })
            .ToListAsync();

        return Ok(fabricantes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FabricanteDto>> GetById(int id)
    {
        var fabricante = await _context.Fabricantes.FindAsync(id);
        if (fabricante is null)
        {
            return NotFound(new { mensagem = $"Fabricante {id} não encontrado." });
        }

        return Ok(ToDto(fabricante));
    }

    [HttpPost]
    public async Task<ActionResult<FabricanteDto>> Create(FabricanteCreateDto dto)
    {
        var fabricante = new Fabricante
        {
            Nome = dto.Nome.Trim(),
            PaisOrigem = dto.PaisOrigem.Trim()
        };

        _context.Fabricantes.Add(fabricante);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um fabricante com esse nome." });
        }

        return CreatedAtAction(nameof(GetById), new { id = fabricante.Id }, ToDto(fabricante));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FabricanteUpdateDto dto)
    {
        var fabricante = await _context.Fabricantes.FindAsync(id);
        if (fabricante is null)
        {
            return NotFound(new { mensagem = $"Fabricante {id} não encontrado." });
        }

        fabricante.Nome = dto.Nome.Trim();
        fabricante.PaisOrigem = dto.PaisOrigem.Trim();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um fabricante com esse nome." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var fabricante = await _context.Fabricantes.FindAsync(id);
        if (fabricante is null)
        {
            return NotFound(new { mensagem = $"Fabricante {id} não encontrado." });
        }

        _context.Fabricantes.Remove(fabricante);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Fabricante possui veículos vinculados e não pode ser removido." });
        }

        return NoContent();
    }
}
