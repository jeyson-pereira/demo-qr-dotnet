using System;
using System.IO;
using DemoQR;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;

internal class Program
{
    private static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        QRGenerator qrGenerator = new QRGenerator(configuration);

        Console.WriteLine("----DEMO QRCode----");

        //Generar random uuid
        string uuid = qrGenerator.GenerateGuid();
        //Generar imagen QR con el contenido en su parametro
        Image<Rgba32> qrImage = qrGenerator.Generate($"https://example.com/{uuid}");

        //Guardado local y en Azure Blob Storage
        string fileName = $"code_qr_{uuid}.png";
        qrGenerator.SaveQRLocal(fileName, qrImage);
        qrGenerator.SaveQRBlob(fileName, qrImage);
    }
}