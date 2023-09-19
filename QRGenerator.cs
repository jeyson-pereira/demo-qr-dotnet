using System;
using System.IO;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;


namespace DemoQR
{
    public class QRGenerator
    {
        private readonly IConfiguration _configuration;

        public QRGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateGuid()
        {
            Guid uuid = Guid.NewGuid();
            string uuidString = uuid.ToString("N").Substring(0, 8);
            return uuidString;

        }
        public Image<Rgba32> Generate(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            // Dato importante para validar el uso de Drawing.Common dependiendo necesidades
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only

            //ImageSharp y manejo de imagen en matriz bytes como alternativa de compatibilidad
            BitmapByteQRCode qrCodeBitmap = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCodeBitmap.GetGraphic(20);

            using (var qrCodeImage = Image.Load(qrCodeBytes))
            {
                return qrCodeImage.CloneAs<Rgba32>();
            }
        }

        public void SaveQRLocal(string fileName, Image<Rgba32> qrImage, string directory = "QRImages")
        {
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, fileName);
            
            qrImage.Save(savePath);
            Console.Write($"\nQRCode saved to: {savePath}");
        }

        public void SaveQRBlob(string blobName, Image<Rgba32> qrCodeImage, string containerName = "qrimages")
        {
            // ConnectionString Azure Blob Storage
            // Cambiar cadena de conexión en appsettings.json
            string connectionString = _configuration.GetConnectionString("AzureStorage");

            //Client service
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Referencia a contenedor y validar creación de contenedor
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            if (!containerClient.Exists())
            {
                containerClient.Create();
            }

            // Referencia del blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Guardar la imagen como blob en Azure Blob Storage
            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.SaveAsPng(stream); // Guardar la imagen en formato PNG en el flujo de memoria
                stream.Seek(0, SeekOrigin.Begin); // Reiniciar el flujo de memoria al principio
                blobClient.Upload(stream, true); // Cargar el flujo de memoria al blob en Azure Storage
            }

            Console.WriteLine($"\nQRCode saved to Azure Blob Storage: {blobClient.Uri}");
        }
    }
       
}
