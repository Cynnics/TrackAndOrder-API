namespace SmartWarehouseAPI.Models.Request
{
    public class ActualizarRutaRequest
    {
        public int? IdRepartidor { get; set; }
        public string? FechaRuta { get; set; }
        public double? DistanciaEstimadaKm { get; set; }
        public int? DuracionEstimadaMin { get; set; }
        public string? Estado { get; set; }
    }
}
