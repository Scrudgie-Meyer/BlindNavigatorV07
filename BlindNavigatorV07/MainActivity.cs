using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Widget;
using Xamarin.Essentials;
using Android.Graphics;
using System.IO;
using Plugin.Media.Abstractions;
using Plugin.Media;

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
            if (ObjectNumber == 1)
            {
                // Initialize the MediaPlayer object
                mediaPlayer = MediaPlayer.Create(this, Resource.Raw.Task1);

                mediaPlayer.Start();
            }
        }
        private async void TakePhoto(int ObjectNumber)
        {
            var photoOptions = new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 40
            };
            var photoFile = await CrossMedia.Current.TakePhotoAsync(photoOptions);

            //Read the image data into a byte array using a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                await photoFile.GetStream().CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();

                // Create a Bitmap from the image data
                Bitmap image = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

                ObjectNumber = ObjectDetection.Detect(image);

                Sound(ObjectNumber);
            }
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
            View view = (View) sender;
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
