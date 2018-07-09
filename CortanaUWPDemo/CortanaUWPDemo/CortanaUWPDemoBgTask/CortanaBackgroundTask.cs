using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace CortanaUWPDemoBgTask
{
    public sealed class CortanaBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            // This should match the uap:AppService and VoiceCommandService references from the 
            // package manifest and VCD files, respectively. Make sure we've been launched by
            // a Cortana Voice Command.
            if (triggerDetails != null && triggerDetails.Name == "CortanaBackgroundTask")
            {
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    var text = voiceCommand.SpeechRecognitionResult.Text;

                    if (text.Equals("I"))
                    {

                        RespondTouser("Hi, How are your doing ?");
                    }
                    else
                    {
                        //Here you call up any api's to create a meaning full response to user/call services like Bot/Luis etc... to create a meaningful response.
                        RespondTouser("You Said ," + text);
                    }
                    
                }
                catch (Exception ex)
                {

                }
            }
        }
        private async void RespondTouser(string text)
        {
            var userMessage = new VoiceCommandUserMessage();
            string keepingTripToDestination = text; //How can i Help you?
            userMessage.DisplayMessage = userMessage.SpokenMessage = keepingTripToDestination;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
    }
}
