using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Plugin.TextToSpeech;
using Syn.Bot.Oscova;
using OscovaAndroidBot.Dialogs;
namespace OscovaAndroidBot
{
    [Activity(Label = "NEURA", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private bool isRecording;
        private readonly int VOICE = 10;
        private EditText input;
        private EditText output;
        private Button button;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            isRecording = false;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var bot = new OscovaBot();
            bot.Dialogs.Add(new HelloBotDialog());
            bot.Trainer.StartTraining();

            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(button.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    input.Text = "No microphone present";
                    button.Enabled = false;
                    return;
                });

                alert.Show();
            }
           
            // Get our button from the layout resource,
            // and attach an event to it
            button = FindViewById<Button>(Resource.Id.MyButton);
            input = FindViewById<EditText>(Resource.Id.editText1);
            output = FindViewById<EditText>(Resource.Id.editText2);

            bot.MainUser.ResponseReceived += (sender, args) =>
            {
                output.Text = $"Bot: {args.Response.Text}";
            };
            button.Click += delegate
            {
                var result = bot.Evaluate(input.Text);
                result.Invoke();
                CrossTextToSpeech.Current.Speak(output.Text);
                // change the text on the button
                button.Text = "End Recording";
                isRecording = !isRecording;
                if (isRecording)
                {
                    // create the intent and start the activity
                    var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                    // put a message on the modal dialog
                    voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                    // if there is more then 1.5s of silence, consider the speech over
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                    // you can specify other languages recognised here, for example
                    // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                    // if you wish it to recognise the default Locale language and German
                    // if you do use another locale, regional dialects may not be recognised very well

                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                    StartActivityForResult(voiceIntent, VOICE);

                }

            };

        }
        protected override void OnActivityResult(int requestCode, Android.App.Result resultVal, Intent data)
        {
            var bot = new OscovaBot();
            bot.Dialogs.Add(new HelloBotDialog());
            bot.Trainer.StartTraining();
            if (requestCode == VOICE)
            {
                if (resultVal == Android.App.Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = input.Text + matches[0];

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                            textInput = textInput.Substring(0, 500);
                        input.Text = textInput;
                        var result = bot.Evaluate(textInput);
                        result.Invoke();
                        CrossTextToSpeech.Current.Speak(output.Text);
                    }
                    else
                        input.Text = "No speech was recognised";
                    // change the text back on the button
                    button.Text = "Start Recording";
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }
    }
}
