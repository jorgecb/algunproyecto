using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilerias
{
    /// <summary>
    /// Clase QR
    /// </summary>
    public class QR
    {
        private readonly QRCodeGenerator _qRCodeGenerator;

        /// <summary>
        /// Constructor
        /// </summary>
        public QR() 
        {
            _qRCodeGenerator = new QRCodeGenerator();
        }

        /// <summary>
        /// Genera código QR en Base64 para HTML
        /// </summary>
        /// <param name="url">Url a condificar</param>
        /// <returns>Cadena en Base64 para HTML</returns>
        public string GenerarQR(string url)
        {
            string respuesta = string.Empty, txtQRCode = string.Empty;

            try
            {
                txtQRCode = url;
                var qrCodeData = _qRCodeGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        var imgBytes = ms.ToArray();
                        respuesta = "data:image/png;base64," + Convert.ToBase64String(imgBytes);
                    }    
                }
            }
            catch
            {
                respuesta = string.Empty;
            }

            return respuesta;
        }
    }
}
