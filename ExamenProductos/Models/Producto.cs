namespace ExamenProductos.Models;

public class Producto
{
    // Id para identificar el registro al editar o eliminar
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public string UrlProducto { get; set; } = string.Empty;

    // Propiedad solo para mostrar en el CollectionView.
    // JsonIgnore evita que se escriba en el JSON.
    [System.Text.Json.Serialization.JsonIgnore]
    public string Resumen =>
        $"Cantidad: {CantidadDisponible}   |   Costo: {PrecioCosto:C}   |   Venta: {PrecioVenta:C}";
}