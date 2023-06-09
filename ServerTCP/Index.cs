﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using System.Collections;
using Microsoft.ML;

namespace YOLOv4MLNet
{
    /// <summary>
    /// Server class, represents a TCP server
    /// </summary>
    class Server
    {
        private TcpListener _listener;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task Start()
        {

            Console.WriteLine($"Server started on port {_listener.LocalEndpoint}");

            //Server set up
            _listener.Start();

            var recognitionModel = new ObjectRecognition();

            //Train model
            recognitionModel.trainModel();

            //Listen to the requests
            while (true)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}");

                    // Handle client request asynchronously
                    _ = Task.Run(() => HandleClient(client, recognitionModel));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client, ObjectRecognition model)
        {
            try
            {
                // Get network stream for reading/writing data
                NetworkStream stream = client.GetStream();

                // Read image data from the client
                byte[] buffer = new byte[64 * 1024];
                await Task.Delay(100);
                int bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                byte[] imageData = new byte[bytesRead];
                Array.Copy(buffer, imageData, bytesRead);

                //Print the size of recieved data
                int byteSize = imageData.Length;
                double kiloSize = (double)byteSize / 1024;

                Console.WriteLine($"{kiloSize} + kB");

                //Logs the data

                Directory.CreateDirectory("logs");
                string folderPath = @"logs";
                string fileName = "image.jpg";
                string filePath = Path.Combine(folderPath, fileName);

                // Check if the file already exists, and if so, generate a unique filename
                int i = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"image({i}).jpg";
                    filePath = Path.Combine(folderPath, fileName);
                    i++;
                }
                Image image;

                using (var ms = new MemoryStream(imageData))
                {
                    image = Image.FromStream(ms);
                    image.Save(filePath);
                }

                image.Dispose();

                // Process image and get list of objects
                var objectsOnImage = model.ProcessImage(imageData);

                // Serialize list of objects to JSON
                string json = JsonConvert.SerializeObject(objectsOnImage);

                // Write JSON data to client
                byte[] data = Encoding.ASCII.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                Console.WriteLine($"Response sent to client {client.Client.RemoteEndPoint}");
                Console.WriteLine($"There were {objectsOnImage.Count} objects on picture");
                // Close the client connection
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public void Stop()
        {
            _listener.Stop();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 8080;
            Server server = new Server(port);
            await server.Start();
        }
    }
}
