using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using System.Drawing;
using Image = iText.Layout.Element.Image;



namespace AppWeb.Controllers
{
    public class MisReservasController : Controller
    {
        private readonly ProyectoContext _context;

        public MisReservasController(ProyectoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var nombreUsuarioActual = User.Identity.Name;
            var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);

            if (usuarioActual == null)
            {
                return NotFound();
            }

            var reservasUsuario = _context.Events
                .Include(e => e.Estado)
                .Include(e => e.Pago)
                .Include(e => e.User)
                .Include(e => e.Vehiculo)
                .Where(e => e.UserId == usuarioActual.Id);

            return View(await reservasUsuario.ToListAsync());
        }

        [HttpGet]
        [Route("MisReservas/GeneratePdf")]
        public async Task<IActionResult> GeneratePdf(int eventId)
        {
            try
            {
                var nombreUsuarioActual = User.Identity.Name;
                var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);

                if (usuarioActual == null)
                {
                    return NotFound();
                }

                string nombreUsuario = usuarioActual.NombreUsuario;

                var reserva = await _context.Events
                    .Include(e => e.Estado)
                    .Include(e => e.Pago)
                    .Include(e => e.User)
                    .Include(e => e.Vehiculo)
                    .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == usuarioActual.Id);

                if (reserva == null)
                {
                    return NotFound();
                }

                decimal precioBase = reserva.Pago?.Monto ?? 0;
                decimal precioAumentado = precioBase * 1.1m; // Aumento del 10%

                var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Establecer márgenes
                document.SetMargins(30, 30, 30, 30);

                // Estilo del título
                Style titleStyle = new Style()
                    .SetFontColor(DeviceRgb.BLACK)
                    .SetFontSize(24)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold();

                // Estilo del texto normal
                Style normalTextStyle = new Style()
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);

                // Agregar un fondo de color al título
                document.Add(new Paragraph("FACTURA DE RESERVA").AddStyle(titleStyle)
                    .SetBackgroundColor(DeviceRgb.WHITE)
                    .SetMarginBottom(20));

                // Agregar imagen desde el modelo Vehiculo
                var palmsCarsParagraph = new Paragraph("PALMS CARS").SetFontColor(DeviceRgb.BLACK).SetFontSize(60).SetTextAlignment(TextAlignment.LEFT);

                // Crear una tabla con dos columnas para la imagen y el texto
                var table2 = new Table(new float[] { 1, 1 }).UseAllAvailableWidth();

                // Agregar la imagen al lado izquierdo de la tabla
                table2.AddCell(new Cell().Add(new Image(ImageDataFactory.Create("https://static.vecteezy.com/system/resources/previews/015/605/709/original/palm-tree-icon-outline-style-vector.jpg")).SetWidth(100)));

                // Agregar el texto "PALMS CARS" al lado derecho de la tabla
                table2.AddCell(new Cell().Add(palmsCarsParagraph));

                document.Add(table2);

                // Establecer el color de fondo de la tabla de información
                var cellBackgroundColor = new DeviceRgb(200, 200, 200);
                var table = new Table(new float[] { 1, 1 }).UseAllAvailableWidth();
                table.SetMarginTop(20);

                table.AddCell(new Cell().Add(new Paragraph("Descripción")).SetBackgroundColor(cellBackgroundColor));
                table.AddCell(new Cell().Add(new Paragraph("Información")).SetBackgroundColor(cellBackgroundColor));

                table.AddCell(new Cell().Add(new Paragraph("Nombres Completos")));
                table.AddCell(new Cell().Add(new Paragraph(nombreUsuario)));

                table.AddCell(new Cell().Add(new Paragraph("Cédula:")));
                table.AddCell(new Cell().Add(new Paragraph(usuarioActual.Cedula)));

                table.AddCell(new Cell().Add(new Paragraph("Correo Electrónico:")));
                table.AddCell(new Cell().Add(new Paragraph(usuarioActual.Correo)));

                table.AddCell(new Cell().Add(new Paragraph("Número de Celular:")));
                table.AddCell(new Cell().Add(new Paragraph(usuarioActual.Celular)));

                table.AddCell(new Cell().Add(new Paragraph("Reserva:")));
                table.AddCell(new Cell().Add(new Paragraph(reserva.Title)));

                table.AddCell(new Cell().Add(new Paragraph("Fecha de Inicio:")));
                table.AddCell(new Cell().Add(new Paragraph(reserva.Start.ToString())));

                table.AddCell(new Cell().Add(new Paragraph("Fecha de Fin:")));
                table.AddCell(new Cell().Add(new Paragraph(reserva.End.ToString())));

                table.AddCell(new Cell().Add(new Paragraph("Monto Total:")));
                table.AddCell(new Cell().Add(new Paragraph(precioAumentado.ToString("0.00"))));

                table.AddCell(new Cell().Add(new Paragraph("Vehículo:")));
                table.AddCell(new Cell().Add(new Paragraph(reserva.Vehiculo?.Nombre)));

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Añadir un agradecimiento o mensaje adicional
                document.Add(new Paragraph("\n¡Gracias por tu reserva! No te olvides de mostrar tu licencia de conducir para entregarte tus llaves").SetTextAlignment(TextAlignment.CENTER).SetFontColor(DeviceRgb.BLACK).SetBold());

                // Añadir la línea "FIRMA DEL CLIENTE"
                document.Add(new Paragraph("\n\n___________________________\nFIRMA DEL CLIENTE").SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                var qrCodeValue = GenerateUniqueQRCodeValue(eventId);  // Debes implementar la generación de un valor único para cada factura
                var qrCodeImage = GenerateQRCodeImage(qrCodeValue);
                document.Add(new Image(ImageDataFactory.Create(qrCodeImage)).SetWidth(100).SetHorizontalAlignment(HorizontalAlignment.CENTER));
                document.Close();

                var fileName = $"Factura_Reserva_{eventId}.pdf";
                var contentType = "application/pdf";

                return File(memoryStream.ToArray(), contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar la factura: {ex.Message}");
                return BadRequest("Error al generar la factura");
            }
        }
        private string GenerateUniqueQRCodeValue(int eventId)
        {
            // Puedes combinar información como el ID del evento, fecha, etc., para crear un valor único
            return $"{eventId}-{DateTime.UtcNow.Ticks}";
        }

        // Función para generar la imagen del código QR
        private byte[] GenerateQRCodeImage(string value)
        {
            var barcodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 200,  // Ajusta el tamaño según tus necesidades
                    Height = 200,
                    Margin = 0
                }
            };

            var pixelData = barcodeWriter.Write(value);

            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using (var stream = new MemoryStream())


                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }


    }
}
