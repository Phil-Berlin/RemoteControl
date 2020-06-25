using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace ClientApp
{
    public class Console : Activity
    {
        private TextView _console;
        private ScrollView _consoleScrollView; 

        public Console(ScrollView scrollView, TextView console)
        {
            _consoleScrollView = scrollView;
            _console = console;
        }

        public void Write(string text)
        {
            WriteAndScroll(text + " ");
        }

        public void WriteLine(string text)
        {
            WriteAndScroll(text + "\n");
        }

        private void WriteAndScroll(string text)
        {
            RunOnUiThread(() => {
                _console.Text += text;
                _consoleScrollView.SmoothScrollTo(0, _console.Bottom);
                });
        }

        public void Clear()
        {
            RunOnUiThread(() => {
                _console.Text = "";
                _consoleScrollView.FullScroll(FocusSearchDirection.Up);
            });
        }
    }
}