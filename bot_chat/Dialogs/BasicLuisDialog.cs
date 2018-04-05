using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace bot_chat.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        // Store notes in a dictionary that uses the title as a key
        private readonly Dictionary<string, Communication> noteByTitle = new Dictionary<string, Communication>();

        [Serializable]
        public sealed class Communication : IEquatable<Communication>
        {

            public string MessageType { get; set; }
            public string ContactName { get; set; }
            public string ContactAttribute { get; set; }

            public override string ToString()
            {
                return $"[{this.MessageType} : {this.ContactName} : {this.ContactAttribute}]";
            }

            public bool Equals(Communication other)
            {
                return other != null
                    && this.ContactName == other.ContactName
                    && this.ContactAttribute == other.ContactAttribute
                    && this.MessageType == other.MessageType;
            }

            public override bool Equals(object other)
            {
                return Equals(other as Communication);
            }

            public override int GetHashCode()
            {
                return this.ContactName.GetHashCode();
            }
        }

        // CONSTANTS        
        // Name of note title entity
        public const string Entity_Communication_ContactName = "Communication.ContactName";
        // Default note title
        public const string DefaultCommunicationContactName = "default";

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}