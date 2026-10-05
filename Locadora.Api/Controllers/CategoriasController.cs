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
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    private static CategoriaDto ToDto(Categoria c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Descricao = c.Descricao,
        ValorDiariaBase = c.ValorDiariaBase
    };

    /// <summary>
    /// Lista todos os categorias.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll()
    {
        var categorias = await _context.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto { Id = c.Id, Nome = c.Nome, Descricao = c.Descricao, ValorDiariaBase = c.ValorDiariaBase })
            .ToListAsync();

        return Ok(categorias);
    }

    /// <summary>
    /// Obtém a categoria pelo Id.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensagem = $"Categoria {id} não encontrada." });
        }

        return Ok(ToDto(categoria));
    }

    /// <summary>
    /// Cadastra a categoria.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CategoriaCreateDto dto)
    {
        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            ValorDiariaBase = dto.ValorDiariaBase
        };

        _context.Categorias.Add(categoria);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe uma categoria com esse nome." });
        }

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, ToDto(categoria));
    }

    /// <summary>
    /// Atualiza a categoria existente.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoriaUpdateDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensagem = $"Categoria {id} não encontrada." });
        }

        categoria.Nome = dto.Nome.Trim();
        categoria.Descricao = dto.Descricao?.Trim();
        categoria.ValorDiariaBase = dto.ValorDiariaBase;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe uma categoria com esse nome." });
        }

        return NoContent();
    }

    /// <summary>
    /// Remove a categoria.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensagem = $"Categoria {id} não encontrada." });
        }

        _context.Categorias.Remove(categoria);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Categoria possui veículos vinculados e não pode ser removida." });
        }

        return NoContent();
    }
}
