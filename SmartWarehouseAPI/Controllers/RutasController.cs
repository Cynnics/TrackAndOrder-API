using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWarehouseAPI.Data;
using SmartWarehouseAPI.Models;
using SmartWarehouseAPI.Models.Request;

[ApiController]
[Route("api/[controller]")]
[Authorize] // requiere token
public class RutasController : ControllerBase
{
    private readonly AppDbContext _context;
    public RutasController(AppDbContext context) { _context = context; }

    // GET: api/Rutas
    [HttpGet]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> GetAll()
    {
        var rutas = await _context.RutasEntrega.ToListAsync();
        return Ok(rutas);
    }

    // GET: api/Rutas/5
    [HttpGet("{id}")]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> Get(int id)
    {
        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound();
        return Ok(ruta);
    }

    // POST: api/Rutas
    [HttpPost]
    [Authorize(Roles = "admin,empleado")]
    public async Task<IActionResult> Create([FromBody] RutaEntrega dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ruta = new RutaEntrega
        {
            IdRepartidor = dto.IdRepartidor,
            FechaRuta = dto.FechaRuta,
            DistanciaEstimadaKm = dto.DistanciaEstimadaKm,
            DuracionEstimadaMin = dto.DuracionEstimadaMin,
            Estado = dto.Estado
        };

        _context.RutasEntrega.Add(ruta);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ruta.IdRuta }, ruta);
    }

    // PATCH: api/Rutas/5/estado
    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> PatchEstado(int id, [FromBody] string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) return BadRequest(new { message = "Estado requerido" });

        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound();

        ruta.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return Ok(ruta);
    }

    // DELETE: api/Rutas/5  (hard delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound();

        // Opcional: comprobar si hay relaciones en RutaPedidos
        var hasAsignados = await _context.RutasPedido.AnyAsync(rp => rp.IdRuta == id);
        if (hasAsignados)
            return BadRequest(new { message = "No se puede eliminar: hay pedidos asignados a la ruta." });

        _context.RutasEntrega.Remove(ruta);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/Rutas/{id}/pedidos/{pedidoId}  -> asignar un pedido a la ruta
    [HttpPost("{id}/pedidos/{pedidoId}")]
    [Authorize(Roles = "admin,empleado")]
    public async Task<IActionResult> AsignarPedido(int id, int pedidoId)
    {
        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound(new { message = "Ruta no encontrada" });

        var pedido = await _context.Pedidos.FindAsync(pedidoId);
        if (pedido == null) return NotFound(new { message = "Pedido no encontrado" });

        // evita duplicados
        var existing = await _context.RutasPedido.FirstOrDefaultAsync(rp => rp.IdRuta == id && rp.IdPedido == pedidoId);
        if (existing != null) return BadRequest(new { message = "Pedido ya asignado a la ruta" });

        var rp = new RutaPedido { IdRuta = id, IdPedido = pedidoId };
        _context.RutasPedido.Add(rp);
        await _context.SaveChangesAsync();

        // opcional: marcar pedido con IdRepartidor y estado en la API si lo deseas
        pedido.IdRepartidor = ruta.IdRepartidor;
        pedido.Estado = "en_reparto";
        await _context.SaveChangesAsync();

        return Ok(rp);
    }

    // GET: api/Rutas/5/pedidos  -> pedidos asignados a ruta
    [HttpGet("{id}/pedidos")]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> GetPedidosDeRuta(int id)
    {
        var lista = await (from rp in _context.RutasPedido
                           join p in _context.Pedidos on rp.IdPedido equals p.IdPedido
                           where rp.IdRuta == id
                           select p).ToListAsync();

        return Ok(lista);
    }

    // GET: api/Rutas/5/ubicaciones -> ubicaciones del repartidor (filtro por fecha si quieres)
    [HttpGet("{id}/ubicaciones")]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> GetUbicacionesDeRuta(int id)
    {
        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound();

        var ubicaciones = await _context.UbicacionesRepartidor
            .Where(u => u.IdRepartidor == ruta.IdRepartidor)
            .OrderByDescending(u => u.FechaHora)
            .ToListAsync();

        return Ok(ubicaciones);
    }

    // PATCH: api/Rutas/5
    [HttpPatch("{id}")]
    [Authorize(Roles = "admin,empleado,repartidor")]
    public async Task<IActionResult> PatchRuta(int id, [FromBody] ActualizarRutaRequest req)
    {
        var ruta = await _context.RutasEntrega.FindAsync(id);
        if (ruta == null) return NotFound();


        ruta.DistanciaEstimadaKm = (decimal?)req.DistanciaEstimadaKm;
        ruta.DuracionEstimadaMin = req.DuracionEstimadaMin;

        await _context.SaveChangesAsync();

        return Ok(ruta);
    }



}
