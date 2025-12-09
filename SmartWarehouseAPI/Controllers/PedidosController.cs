using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWarehouseAPI.Data;
using SmartWarehouseAPI.Models;
using SmartWarehouseAPI.Models.Request;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Pedidos/
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound();

        return Ok(pedido);
    }


    // 🔹 GET con filtro opcional
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos([FromQuery] string estado = null)
    {
        var query = _context.Pedidos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado.ToLower() == estado.ToLower());

        return await query.ToListAsync();
    }

    // 🔹 GET: pedidos entregados
    [HttpGet("entregados")]
    [Authorize]
    public async Task<ActionResult<List<Pedido>>> GetEntregados()
    {
        return await _context.Pedidos
            .Where(p => p.Estado == "entregado")
            .ToListAsync();
    }


    // 🔹 PATCH: cambiar estado
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstadoPedido(int id, [FromBody] string nuevoEstado)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound(new { message = "Pedido no encontrado." });

        pedido.Estado = nuevoEstado;

        if (nuevoEstado.ToLower() == "entregado")
            pedido.FechaEntrega = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Estado actualizado a {nuevoEstado}" });
    }



    // 🔹 POST: crear pedido
    [HttpPost]
    [Authorize(Roles = "admin,empleado,cliente")]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        pedido.FechaPedido = DateTime.Now;
        pedido.Estado = "pendiente";

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedidos), new { id = pedido.IdPedido }, pedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPedido(int id, Pedido model)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound();

        pedido.Estado = model.Estado;
        pedido.IdCliente = model.IdCliente;
        pedido.IdRepartidor = model.IdRepartidor;
        pedido.FechaPedido = model.FechaPedido;
        pedido.FechaEntrega = model.FechaEntrega;

        await _context.SaveChangesAsync();
        return Ok(pedido);
    }


    // 🔹 GET: totales de un pedido
    [HttpGet("{id}/totales")]
    [Authorize]
    public async Task<IActionResult> GetTotalesPedido(int id)
    {
        var detalles = await _context.DetallesPedido
            .Where(d => d.IdPedido == id)
            .ToListAsync();

        if (!detalles.Any())
            return NotFound(new { message = "Este pedido no tiene detalles." });

        decimal subtotal = detalles.Sum(d => d.Subtotal);
        decimal iva = subtotal * 0.21m;
        decimal total = subtotal + iva;

        return Ok(new { subtotal, iva, total });
    }

    // DELETE: Eliminar pedido
    [HttpDelete("{id}")]    
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { message = "Pedido no encontrado" });

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }


}
