using System;
using System.Drawing;
using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using Google.Android.Material.Snackbar;
using Android.Media;
using Android.Widget;
using Xamarin.Essentials;
using System.IO;
using Plugin.Media.Abstractions;
using Plugin.Media;
using System.Linq;
using System.Collections.Generic;
using Android.Graphics;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YOLOv4MLNet.DataStructures;
using Java.Util;

namespace BlindNavigatorV07
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Button button1;

        private MediaPlayer mediaPlayer;
        protected override async void OnCreate(Bundle savedInstanceState)
        {

            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            await Permissions.RequestAsync<Permissions.Camera>();
            await Permissions.RequestAsync<Permissions.Photos>();
            await Permissions.RequestAsync<Permissions.Speech>();

            button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Text = "Start";
            button1.Click += Button1_Click;


        }
        public void Button1_Click(object sender, EventArgs e)
        {
            bool IsStarted;
            if (button1.Text == "Start")
                IsStarted = true;

            else IsStarted = false;


            if (IsStarted == true)
            {
                button1.Text = "Program is started"; //Алгоритм запущено
                Procedure();
            }
            else button1.Text = "Start"; //Алгоритм зупинено

        }
        private void Procedure()
        {
            int ObjectNumber = 0;

            TakePhoto(ObjectNumber);
        }
        private void Sound(int ObjectNumber)
        {
            throw new NotImplementedException();
        }
        private async Task<List<YoloV4Result>> TakePhoto(int ObjectNumber)
        {
            var photoOptions = new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 40
            };
            var photoFile = await CrossMedia.Current.TakePhotoAsync(photoOptions);
            var Result = new List<YoloV4Result>();

            IPAddress ServerIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            const int ServerPort = 8080;

            //Read the image data into a byte array using a MemoryStream
            using (var memoryStream = new System.IO.MemoryStream())
            {

                await photoFile.GetStream().CopyToAsync(memoryStream);

                var ImageByte = memoryStream.ToArray();

                string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                string fileName = "image.jpg";
                string filePath = System.IO.Path.Combine(folderPath, fileName);

                // Check if the file already exists, and if so, generate a unique filename
                int i = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"image({i}).jpg";
                    filePath = System.IO.Path.Combine(folderPath, fileName);
                    i++;
                }

                File.WriteAllBytes(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName), ImageByte);

                TcpClient client = new TcpClient();
                await client.ConnectAsync("10.0.2.2", ServerPort);

                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(ImageByte, 0, ImageByte.Length);

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                string json = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                client.Close();

                Result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<YoloV4Result>>(json);

            }
            return Result;
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
