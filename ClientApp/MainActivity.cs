using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Telecom;
using Android.Views;
using Android.Widget;
using CommandAndControl;
using Java.Interop;
using Java.Lang;
using Java.Net;

using Command = CommandAndControl.Commands.Command;

namespace ClientApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Console console;

        Button buttonShutdown;
        Button buttonSendMessage;
        Button buttonPing;
        EditText editTextMessage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            buttonShutdown = FindViewById<Button>(Resource.Id.buttonShutdown);
            buttonShutdown.Click += OnButtonShutdownClick;

            buttonSendMessage = FindViewById<Button>(Resource.Id.buttonSendMessage);
            buttonSendMessage.Click += OnButtonSendMessageClick;

            buttonPing = FindViewById<Button>(Resource.Id.buttonPing);
            buttonPing.Click += OnButtonPingClick;

            editTextMessage = FindViewById<EditText>(Resource.Id.editTextMessage);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fabClearScreen);
            fab.Click += OnFabClick;

            console = new Console(FindViewById<ScrollView>(Resource.Id.consoleScrollView), FindViewById<TextView>(Resource.Id.consoleText));
            console.WriteLine("Remote Control Client - Console");
        }

        public void OnButtonPingClick(object sender, EventArgs eventArgs)
        {
            console.Write("Sending Ping... ");
            byte[] data = new byte[] { (byte)Command.Ping };
            SendBytes(data);
        }

        public void OnButtonShutdownClick(object sender, EventArgs eventArgs)
        {
            string text = editTextMessage.Text;
            byte[] data;

            if (text != "")
            {
                console.WriteLine("Sending Shutdown with Text: " + text);

                data = new byte[1 + text.Length];
                data[0] = (byte)Command.Shutdown;

                try
                {
                    Encoding.UTF8.GetBytes(text).CopyTo(data, 1);
                }
                catch (System.Exception e)
                {
                    console.WriteLine("ERROR: " + e.Message);
                }
            }
            else
            {
                console.WriteLine("Sending Shutdown");

                data = new byte[1];
                data[0] = (byte)Command.Shutdown;
            }

            SendBytes(data);
        }

        public void OnButtonSendMessageClick(object sender, EventArgs eventArgs)
        {
            string text = editTextMessage.Text;

            if (text == "")
            {
                console.WriteLine("No Text to send");
                return;
            }

            console.WriteLine("Sending Message: " + text);

            byte[] data = new byte[text.Length + 1];
            data[0] = (byte)Command.SendMessage;

            try
            {
                Encoding.UTF8.GetBytes(text).CopyTo(data, 1);
            }
            catch (System.Exception e)
            {
                console.WriteLine("ERROR: " + e.Message);
            }

            SendBytes(data);
        }

        private void SendBytes(byte[] data)
        {
            ThreadPool.QueueUserWorkItem(async (data) =>
            {

                TcpClient client = new TcpClient();

                try
                {
                    byte[] payload = (byte[])data;
                    await client.ConnectAsync(Network.IP, Network.Port);
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(payload, 0, payload.Length);

                        byte[] response = new byte[2];
                        stream.Read(response, 0, 2);

                        if (response[1] == 1)
                        {
                            console.WriteLine("OK");
                        }
                        else
                        {
                            console.WriteLine("Failed");
                        }
                    }
                    else
                    {
                        console.WriteLine("Not connected");
                    }
                    client.Close();
                }
                catch (System.Exception e)
                {
                    console.WriteLine("\nERROR: " + e.Message);
                }
            }, data);
        }

        public void OnFabClick(object sender, EventArgs eventArgs)
        {
            console.Clear();
            console.WriteLine("Remote Control Client - Console");
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
