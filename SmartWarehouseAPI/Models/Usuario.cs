using System.ComponentModel.DataAnnotations;

namespace SmartWarehouseAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // ADMIN, EMPLEADO, REPARTIDOR, CLIENTE
        public string Telefono { get; set; }
        public string? DireccionFacturacion { get; set; }
        public string? nif { get; set; }
    }
}
