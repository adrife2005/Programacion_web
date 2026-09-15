using System.Collections.ObjectModel;
using System.Globalization;
using ExamenProductos.Models;
using ExamenProductos.Services;

namespace ExamenProductos;

public partial class MainPage : ContentPage
{
    // Colección enlazada al CollectionView
    readonly ObservableCollection<Producto> productos = new();

    // Producto seleccionado (null cuando se captura uno nuevo)
    Producto? seleccionado;

    public MainPage()
    {
        InitializeComponent();

        listaProductos.ItemsSource = productos;

        // Al iniciar: Preferences -> JSON -> Lista -> CollectionView
        CargarDesdePreferences();
    }

    // ========== CARGA Y GUARDADO ==========

    void CargarDesdePreferences()
    {
        productos.Clear();

        foreach (var p in ProductoService.Cargar())
            productos.Add(p);

        ActualizarContador();
    }

    void GuardarEnPreferences()
    {
        // Se guarda SIEMPRE la colección completa serializada en JSON
        ProductoService.Guardar(productos);
        ActualizarContador();
    }

    void ActualizarContador()
    {
        lblContador.Text = productos.Count == 1
            ? "1 producto registrado"
            : $"{productos.Count} productos registrados";
    }

    // ========== SELECCIÓN ==========

    void listaProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as Producto;

        if (item == null)
            return;

        seleccionado = item;

        // Se muestran los datos del registro seleccionado en los controles
        lblTituloForm.Text = $"Producto seleccionado: {item.Nombre}";
        txtNombre.Text = item.Nombre;
        txtDescripcion.Text = item.Descripcion;
        txtCantidad.Text = item.CantidadDisponible.ToString();
        txtCosto.Text = item.PrecioCosto.ToString(CultureInfo.InvariantCulture);
        txtVenta.Text = item.PrecioVenta.ToString(CultureInfo.InvariantCulture);
        txtUrl.Text = item.UrlProducto;
        imgVistaPrevia.Source = string.IsNullOrWhiteSpace(item.UrlProducto) ? null : item.UrlProducto;

        btnEliminar.IsVisible = true;
        panelFormulario.IsVisible = true;
    }

    // ========== NUEVO ==========

    void btnNuevo_Clicked(object sender, EventArgs e)
    {
        seleccionado = null;

        lblTituloForm.Text = "Nuevo producto";
        LimpiarFormulario();

        btnEliminar.IsVisible = false;   // todavía no hay nada que eliminar
        panelFormulario.IsVisible = true;
    }

    void LimpiarFormulario()
    {
        txtNombre.Text = string.Empty;
        txtDescripcion.Text = string.Empty;
        txtCantidad.Text = string.Empty;
        txtCosto.Text = string.Empty;
        txtVenta.Text = string.Empty;
        txtUrl.Text = string.Empty;
        imgVistaPrevia.Source = null;
    }

    // ========== GUARDAR (crear o actualizar) ==========

    async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        // --- VALIDACIONES ---
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            await DisplayAlert("Validación", "El nombre del producto es obligatorio.", "OK");
            txtNombre.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
        {
            await DisplayAlert("Validación", "La descripción es obligatoria.", "OK");
            txtDescripcion.Focus();
            return;
        }

        if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad < 0)
        {
            await DisplayAlert("Validación", "La cantidad debe ser un entero mayor o igual a 0.", "OK");
            txtCantidad.Focus();
            return;
        }

        if (!decimal.TryParse(txtCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo) || costo < 0)
        {
            await DisplayAlert("Validación", "El precio de costo debe ser un número válido.", "OK");
            txtCosto.Focus();
            return;
        }

        if (!decimal.TryParse(txtVenta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal venta) || venta < 0)
        {
            await DisplayAlert("Validación", "El precio de venta debe ser un número válido.", "OK");
            txtVenta.Focus();
            return;
        }

        string url = txtUrl.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(url) && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            await DisplayAlert("Validación", "La URL no tiene un formato válido.", "OK");
            txtUrl.Focus();
            return;
        }

        if (seleccionado == null)
        {
            // --- CREAR ---
            var nuevo = new Producto
            {
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                CantidadDisponible = cantidad,
                PrecioCosto = costo,
                PrecioVenta = venta,
                UrlProducto = url
            };

            productos.Add(nuevo);
        }
        else
        {
            // --- ACTUALIZAR ---
            seleccionado.Nombre = txtNombre.Text.Trim();
            seleccionado.Descripcion = txtDescripcion.Text.Trim();
            seleccionado.CantidadDisponible = cantidad;
            seleccionado.PrecioCosto = costo;
            seleccionado.PrecioVenta = venta;
            seleccionado.UrlProducto = url;

            // Se reemplaza el elemento para que el CollectionView se refresque
            int indice = productos.IndexOf(seleccionado);
            productos[indice] = seleccionado;
        }

        GuardarEnPreferences();   // Lista -> JSON -> Preferences
        CerrarFormulario();

        await DisplayAlert("Listo", "La información fue guardada correctamente.", "OK");
    }

    // ========== ELIMINAR ==========

    async void btnEliminar_Clicked(object sender, EventArgs e)
    {
        if (seleccionado == null)
            return;

        bool confirmar = await DisplayAlert(
            "Confirmar",
            $"¿Está seguro de eliminar el producto \"{seleccionado.Nombre}\"?",
            "Eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        productos.Remove(seleccionado);

        GuardarEnPreferences();
        CerrarFormulario();

        await DisplayAlert("Listo", "El producto fue eliminado.", "OK");
    }

    // ========== CANCELAR ==========

    void btnCancelar_Clicked(object sender, EventArgs e) => CerrarFormulario();

    void CerrarFormulario()
    {
        panelFormulario.IsVisible = false;
        seleccionado = null;
        listaProductos.SelectedItem = null;   // permite volver a seleccionar el mismo registro
        LimpiarFormulario();
    }

    // ========== EXTRAS ==========

    void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
    {
        string url = e.NewTextValue?.Trim() ?? string.Empty;
        imgVistaPrevia.Source = Uri.IsWellFormedUriString(url, UriKind.Absolute) ? url : null;
    }

    // Para demostrarle al profesor que sí se usa JSON
    async void btnVerJson_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("JSON almacenado en Preferences", ProductoService.ObtenerJson(), "Cerrar");
    }
}