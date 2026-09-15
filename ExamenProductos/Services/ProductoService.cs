using System.Text.Json;
using ExamenProductos.Models;

namespace ExamenProductos.Services;

public static class ProductoService
{
    // Llave con la que se guarda el JSON dentro de Preferences
    const string Llave = "productos";

    static readonly JsonSerializerOptions Opciones = new() { WriteIndented = true };

    // Preferences -> JSON -> Lista de objetos
    public static List<Producto> Cargar()
    {
        try
        {
            string json = Preferences.Get(Llave, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
                return new List<Producto>();

            return JsonSerializer.Deserialize<List<Producto>>(json) ?? new List<Producto>();
        }
        catch
        {
            // Si el JSON estuviera dañado, se inicia con lista vacía
            return new List<Producto>();
        }
    }

    // Lista de objetos -> JSON -> Preferences
    public static void Guardar(IEnumerable<Producto> productos)
    {
        string json = JsonSerializer.Serialize(productos, Opciones);
        Preferences.Set(Llave, json);
    }

    // Devuelve el JSON tal cual está guardado (para demostrarlo al profesor)
    public static string ObtenerJson() => Preferences.Get(Llave, "[]");
}