using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWarehouseAPI.Data;
using SmartWarehouseAPI.Models;
using System.Text.Json;

namespace SmartWarehouseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productos
        // ❗ Solo productos activos
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            return Ok(await _context.Productos
                .Where(p => p.Activo == true)
                .ToListAsync());
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var prod = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == id && p.Activo == true);

            if (prod == null) return NotFound();
            return Ok(prod);
        }

        // POST: api/productos
        // ✔ Crear producto nuevo
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, producto);
        }


        // PATCH: api/productos/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProducto(int id, [FromBody] JsonElement cambios)
        {
            var existing = await _context.Productos.FindAsync(id);
            if (existing == null || !existing.Activo)
                return NotFound();

            // 🔹 Extraemos solo los campos enviados en el JSON
            if (cambios.TryGetProperty("nombre", out var nombre) &&
                nombre.ValueKind != JsonValueKind.Null)
                existing.Nombre = nombre.GetString();

            if (cambios.TryGetProperty("descripcion", out var descripcion) &&
                descripcion.ValueKind != JsonValueKind.Null)
                existing.Descripcion = descripcion.GetString();

            if (cambios.TryGetProperty("precio", out var precio) &&
                precio.TryGetDecimal(out var precioValue))
                existing.Precio = precioValue;

            if (cambios.TryGetProperty("stock", out var stock) &&
                stock.TryGetInt32(out var stockValue))
                existing.Stock = stockValue;

            if (cambios.TryGetProperty("categoria", out var categoria) &&
                categoria.ValueKind != JsonValueKind.Null)
                existing.Categoria = categoria.GetString();

            await _context.SaveChangesAsync();
            return Ok(existing);
        }


        // DELETE: api/productos/5
        // ⚠ Soft delete en vez de eliminación real
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var prod = await _context.Productos.FindAsync(id);
            if (prod == null) return NotFound();

            // SOFT DELETE → NO BORRAR FILA
            prod.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto desactivado correctamente" });
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int nuevoStock)
        {
            Console.WriteLine($"📦 PATCH /api/Productos/{id}/stock - Nuevo stock: {nuevoStock}");

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                Console.WriteLine($"❌ Producto {id} no encontrado");
                return NotFound(new { message = "Producto no encontrado" });
            }

            var stockAnterior = producto.Stock;
            producto.Stock = nuevoStock;
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Stock actualizado: {stockAnterior} → {nuevoStock}");
            return NoContent();
        }
    }
}
