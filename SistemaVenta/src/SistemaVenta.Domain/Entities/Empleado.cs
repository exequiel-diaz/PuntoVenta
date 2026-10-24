using SistemaVenta.Domain.Entities;

public class Empleado
{
    public int Id { get; set; }

    public int PersonaId { get; set; }
    public Persona Persona { get; set; } = null!;

    public DateTime FechaIngreso { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
}