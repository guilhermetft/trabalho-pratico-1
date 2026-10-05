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
public class VeiculosController : ControllerBase
{
    private readonly AppDbContext _context;

    public VeiculosController(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<VeiculoDto> Projetar(IQueryable<Veiculo> query) => query
        .Select(v => new VeiculoDto
        {
            Id = v.Id,
            Modelo = v.Modelo,
            Placa = v.Placa,
            AnoFabricacao = v.AnoFabricacao,
            Quilometragem = v.Quilometragem,
            Status = v.Status,
            FabricanteId = v.FabricanteId,
            FabricanteNome = v.Fabricante.Nome,
            CategoriaId = v.CategoriaId,
            CategoriaNome = v.Categoria.Nome,
            ValorDiariaBase = v.Categoria.ValorDiariaBase
        });

    /// <summary>
    /// Lista todos os veiculos.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> GetAll()
    {
        var veiculos = await Projetar(_context.Veiculos.AsQueryable())
            .OrderBy(v => v.Modelo)
            .ToListAsync();

        return Ok(veiculos);
    }

    /// <summary>
    /// Obtém o veículo pelo Id.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeiculoDto>> GetById(int id)
    {
        var veiculo = await Projetar(_context.Veiculos.Where(v => v.Id == id)).FirstOrDefaultAsync();
        if (veiculo is null)
        {
            return NotFound(new { mensagem = $"Veículo {id} não encontrado." });
        }

        return Ok(veiculo);
    }

    /// <summary>
    /// Filtro 1: veículos disponíveis, com filtros opcionais de categoria e fabricante.
    /// INNER JOIN Veiculos x Categorias x Fabricantes (via Include, todas as FKs são obrigatórias).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("disponiveis")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> GetDisponiveis([FromQuery] int? categoriaId, [FromQuery] int? fabricanteId)
    {
        var query = _context.Veiculos
            .Include(v => v.Categoria)
            .Include(v => v.Fabricante)
            .Where(v => v.Status == StatusVeiculo.Disponivel);

        if (categoriaId is not null)
        {
            query = query.Where(v => v.CategoriaId == categoriaId);
        }

        if (fabricanteId is not null)
        {
            query = query.Where(v => v.FabricanteId == fabricanteId);
        }

        var veiculos = await Projetar(query)
            .OrderBy(v => v.Modelo)
            .ToListAsync();

        return Ok(veiculos);
    }

    /// <summary>
    /// Filtro 2: veículos de um fabricante específico, com dados da categoria.
    /// INNER JOIN Veiculos x Fabricantes x Categorias (via Include).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("por-fabricante/{fabricanteId:int}")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> GetPorFabricante(int fabricanteId)
    {
        var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == fabricanteId);
        if (!fabricanteExiste)
        {
            return NotFound(new { mensagem = $"Fabricante {fabricanteId} não encontrado." });
        }

        var query = _context.Veiculos
            .Include(v => v.Fabricante)
            .Include(v => v.Categoria)
            .Where(v => v.FabricanteId == fabricanteId);

        var veiculos = await Projetar(query)
            .OrderBy(v => v.Modelo)
            .ToListAsync();

        return Ok(veiculos);
    }

    /// <summary>
    /// Cadastra o veículo.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> Create(VeiculoCreateDto dto)
    {
        var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == dto.FabricanteId);
        if (!fabricanteExiste)
        {
            return BadRequest(new { mensagem = $"Fabricante {dto.FabricanteId} não encontrado." });
        }

        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExiste)
        {
            return BadRequest(new { mensagem = $"Categoria {dto.CategoriaId} não encontrada." });
        }

        var veiculo = new Veiculo
        {
            Modelo = dto.Modelo.Trim(),
            Placa = dto.Placa.Trim().ToUpperInvariant(),
            AnoFabricacao = dto.AnoFabricacao,
            Quilometragem = dto.Quilometragem,
            Status = dto.Status,
            FabricanteId = dto.FabricanteId,
            CategoriaId = dto.CategoriaId
        };

        _context.Veiculos.Add(veiculo);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um veículo com essa placa." });
        }

        var criado = await Projetar(_context.Veiculos.Where(v => v.Id == veiculo.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = veiculo.Id }, criado);
    }

    /// <summary>
    /// Atualiza o veículo existente.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VeiculoUpdateDto dto)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null)
        {
            return NotFound(new { mensagem = $"Veículo {id} não encontrado." });
        }

        var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == dto.FabricanteId);
        if (!fabricanteExiste)
        {
            return BadRequest(new { mensagem = $"Fabricante {dto.FabricanteId} não encontrado." });
        }

        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExiste)
        {
            return BadRequest(new { mensagem = $"Categoria {dto.CategoriaId} não encontrada." });
        }

        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Placa = dto.Placa.Trim().ToUpperInvariant();
        veiculo.AnoFabricacao = dto.AnoFabricacao;
        veiculo.Quilometragem = dto.Quilometragem;
        veiculo.Status = dto.Status;
        veiculo.FabricanteId = dto.FabricanteId;
        veiculo.CategoriaId = dto.CategoriaId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Já existe um veículo com essa placa." });
        }

        return NoContent();
    }

    /// <summary>
    /// Remove o veículo.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null)
        {
            return NotFound(new { mensagem = $"Veículo {id} não encontrado." });
        }

        _context.Veiculos.Remove(veiculo);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Veículo possui aluguéis vinculados e não pode ser removido." });
        }

        return NoContent();
    }
}
