/*
 * ENCABEZADO DE AUTORÍA
 * Equipo N°: 3
 * Integrantes:
 * 1. CONTRERAS Rodriguez Janis Isabel
 * 2. MACÍAS Cruz Meredith Miranda
 * 3. TORRES Barrera Jose Angel
 */
using Biblioteca.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Biblioteca
{
    // Actividad 3.3: Diálogos y Ventanas Modales.
    // Esta parte de FrmPrincipal concentra los cuadros de diálogo del sistema
    // (SaveFileDialog, ColorDialog, FontDialog), MessageBox y la apertura de ventanas
    // modales (ShowDialog) y no modales (Show).
    public partial class FrmPrincipal
    {
        private FrmConsultaLibros? _consultaAbierta;

        private void ConfigurarDialogos()
        {
            // Ventanas: no modal (Show) y modal (ShowDialog)
            pnlCabezera.Controls.Add(CrearBoton("btnConsulta", "Consulta", 24, 47, 100, btnConsulta_Click));
            pnlCabezera.Controls.Add(CrearBoton("btnEdicionRapida", "Edición rápida", 130, 47, 130, btnEdicionRapida_Click));

            // Diálogos del sistema
            pnlCabezera.Controls.Add(CrearBoton("btnExportar", "Exportar", 722, 47, 94, btnExportar_Click));
            pnlCabezera.Controls.Add(CrearBoton("btnColor", "Color", 822, 47, 94, btnColor_Click));
            pnlCabezera.Controls.Add(CrearBoton("btnFuente", "Fuente", 922, 47, 94, btnFuente_Click));

            // Confirmación al cerrar la aplicación
            this.FormClosing += FrmPrincipal_FormClosing;
        }

        private static Button CrearBoton(string nombre, string texto, int x, int y, int ancho, EventHandler click)
        {
            Button boton = new Button();
            boton.Name = nombre;
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Size = new Size(ancho, 29);
            boton.UseVisualStyleBackColor = true;
            boton.Click += click;
            return boton;
        }

        // ================== SaveFileDialog: exportar el listado de libros ==================
        private void btnExportar_Click(object? sender, EventArgs e)
        {
            if (Libros.ObtenerTodos().Count == 0)
            {
                MessageBox.Show("No hay libros registrados para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Exportar listado de libros";
            dialogo.Filter = "Archivo de texto (*.txt)|*.txt|Archivo CSV (*.csv)|*.csv";
            dialogo.FileName = "Libros";
            dialogo.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialogo.OverwritePrompt = true;

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                bool esCsv = dialogo.FilterIndex == 2;
                string separador = esCsv ? "," : "\t";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Join(separador, "ID", "Titulo", "PrecioDia", "Novedad", "Estado", "Ruta imagen"));

                foreach (Libros l in Libros.ObtenerTodos())
                {
                    sb.AppendLine(string.Join(separador,
                        l.Id,
                        Campo(l.Titulo, esCsv),
                        l.PrecioRentaDiaria.ToString("F2"),
                        l.EsNovedad ? "Si" : "No",
                        l.EsActivo ? "Activo" : "Inactivo",
                        Campo(l.RutaImagen, esCsv)));
                }

                File.WriteAllText(dialogo.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show($"Se exportaron {Libros.ObtenerTodos().Count} libro(s) a:\n{dialogo.FileName}",
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el archivo.\n\n{ex.Message}",
                    "Error al exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // En CSV se entrecomilla el texto para que una coma dentro del título no rompa las columnas
        private static string Campo(string texto, bool esCsv)
        {
            return esCsv ? "\"" + texto.Replace("\"", "\"\"") + "\"" : texto;
        }

        // ================== ColorDialog: color de fondo de los paneles ==================
        private void btnColor_Click(object? sender, EventArgs e)
        {
            using ColorDialog dialogo = new ColorDialog();
            dialogo.Color = pnlCabezera.BackColor;
            dialogo.FullOpen = true;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                pnlCabezera.BackColor = dialogo.Color;
                pnlBotonera.BackColor = dialogo.Color;

                // Las etiquetas tenían fondo fijo; se igualan para que no se vean como "parches"
                foreach (Control c in pnlCabezera.Controls)
                    if (c is Label)
                        c.BackColor = dialogo.Color;
            }
        }

        // ================== FontDialog: estilo de la etiqueta del título ==================
        private void btnFuente_Click(object? sender, EventArgs e)
        {
            using FontDialog dialogo = new FontDialog();
            dialogo.Font = label6.Font;
            dialogo.ShowColor = true;
            dialogo.Color = label6.ForeColor;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                label6.Font = dialogo.Font;
                label6.ForeColor = dialogo.Color;
            }
        }

        // ================== Show: ventana NO modal (se puede seguir usando el formulario principal) ==================
        private void btnConsulta_Click(object? sender, EventArgs e)
        {
            // Solo una instancia: si ya está abierta, se refresca y se trae al frente
            if (_consultaAbierta != null && !_consultaAbierta.IsDisposed)
            {
                _consultaAbierta.Actualizar();
                _consultaAbierta.BringToFront();
                _consultaAbierta.Focus();
                return;
            }

            _consultaAbierta = new FrmConsultaLibros();
            _consultaAbierta.Show(this);
        }

        // ================== ShowDialog: ventana MODAL (bloquea el formulario principal) ==================
        private void btnEdicionRapida_Click(object? sender, EventArgs e)
        {
            using FrmEdicionRapida dialogo = new FrmEdicionRapida();
            DialogResult resultado = dialogo.ShowDialog(this);

            if (resultado != DialogResult.OK)
            {
                EscribirEstado("Edición rápida cancelada.");
                return;
            }

            Libros? libro = Libros.ObtenerTodos().FirstOrDefault(l => l.Id == dialogo.IdLibro);

            if (libro == null)
            {
                MessageBox.Show($"No existe un libro con el ID {dialogo.IdLibro}.", "Edición rápida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            libro.PrecioRentaDiaria = dialogo.NuevoPrecio;
            EscribirEstado($"Precio del libro [{libro.Id}] actualizado a {dialogo.NuevoPrecio:C}.");
            MessageBox.Show("El precio se actualizó correctamente.", "Edición rápida", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_consultaAbierta != null && !_consultaAbierta.IsDisposed)
                _consultaAbierta.Actualizar();
        }

        private void EscribirEstado(string mensaje)
        {
            if (statusStrip1 != null && statusStrip1.Items.Count > 0)
                statusStrip1.Items[0].Text = mensaje;
        }

        // ================== MessageBox: confirmación al cerrar ==================
        private void FrmPrincipal_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                return;

            DialogResult respuesta = MessageBox.Show(
                "¿Desea salir del sistema de Gestión de Biblioteca?\nLos datos capturados se perderán.",
                "Confirmar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.No)
                e.Cancel = true;
        }
    }
}
