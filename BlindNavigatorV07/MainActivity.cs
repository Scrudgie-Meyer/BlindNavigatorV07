using System;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YOLOv4MLNet.DataStructures;
using Java.Util;
using static Android.Content.ClipData;

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

        private async Task Procedure()
        {
            var result = await TakePhoto();

            await Sound(result);

        }
        private async Task Sound(List<YoloV4Result> result)
        {

            var dict = new Dictionary<string, int>();

            //Map the resualt of object recognition
            if(result != null)
            {
                foreach (var resultItem in result)
                {
                    if (dict.ContainsKey(resultItem.Label))
                    {
                        dict[resultItem.Label]++;
                    }
                    else
                    {
                        dict.Add(resultItem.Label, 1);
                    }
                }
            }

            //Text to speech options
            var settings = new SpeechOptions()
            {
                Volume = .75f,
                Pitch = 1.0f
            };

            if(dict.Count == 0)
            {
                await TextToSpeech.SpeakAsync("There is no object on the photo", settings);
            }
            else
            {
                await TextToSpeech.SpeakAsync("There are ", settings);
                foreach (var item in dict.Keys)
                {
                    if (dict[item] == 1)
                    {
                        await TextToSpeech.SpeakAsync($"1 {item}, ", settings);
                    }
                    else
                    {
                        await TextToSpeech.SpeakAsync($"{dict[item]} {item}s, ", settings);
                    }
                }
                await TextToSpeech.SpeakAsync(" on the photo", settings);
            }
            

        }
        private async Task<List<YoloV4Result>> TakePhoto()
        {
            //Take photo on the phone
            var photoOptions = new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Large,
                CompressionQuality = 60
            };
            var photoFile = await CrossMedia.Current.TakePhotoAsync(photoOptions);
            
            var Result = new List<YoloV4Result>();
            const int ServerPort = 8080;
            const string ServerIp = "10.0.2.2";

            //Read the image data into a byte array using a MemoryStream
            using (var memoryStream = new System.IO.MemoryStream())
            {

                await photoFile.GetStream().CopyToAsync(memoryStream);

                //Convert image to byte array
                var ImageByte = memoryStream.ToArray();

                //Make a connection to the server
                TcpClient client = new TcpClient();
                await client.ConnectAsync(ServerIp, ServerPort);

                //Send image to the server
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(ImageByte, 0, ImageByte.Length);

                //Receive resualts from the server
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                string json = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

                //Close the connection
                client.Close();

                //Deserialize JSON object
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
