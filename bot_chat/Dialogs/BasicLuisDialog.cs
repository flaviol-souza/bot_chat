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
    [LuisModel("64af4146-e24f-431a-9bc4-77430ea5fe08", "c0983224e3704b58ad9acd6117d79ee2")]
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        // Store notes in a dictionary that uses the title as a key
        protected readonly Dictionary<string, Communication> communicationByContact = new Dictionary<string, Communication>();
        protected readonly Dictionary<string, Dialogs.Entity.Note> noteByTitle = new Dictionary<string, Dialogs.Entity.Note>();

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

        // CONSTANTS
        // Name of note title entity
        public const string Entity_Communication_Name = "Communication.ContactName";
        // Default note title
        public const string DefaultCommunicationName = "default";


        private Communication contactToCreate;
        private string currentName;
        [LuisIntent("Communication.AddContact")]
        public Task AddContactIntent(IDialogContext context, LuisResult result)
        {
            EntityRecommendation name;
            if (!result.TryFindEntity(Entity_Communication_Name, out name))
            {
                // Prompt the user for a note title
                PromptDialog.Text(context, After_NamePrompt, "What is a contact of the note you want to create?");
            }
            else
            {
                var comm = new Communication() { ContactName = name.Entity };
                contactToCreate = this.communicationByContact[comm.ContactName] = comm;

                // Prompt the user for what they want to say in the note           
                PromptDialog.Text(context, After_NamePrompt, "What do you want to say in your contact?");
            }

            return Task.CompletedTask;
        }

        private async Task After_NamePrompt(IDialogContext context, IAwaitable<string> result)
        {
            EntityRecommendation name;
            // Set the title (used for creation, deletion, and reading)
            currentName = await result;
            if (currentName != null)
            {
                name = new EntityRecommendation(type: Entity_Communication_Name) { Entity = currentName };
            }
            else
            {
                // Use the default note title
                name = new EntityRecommendation(type: Entity_Communication_Name) { Entity = DefaultCommunicationName };
            }

            // Create a new note object 
            var comm = new Communication() { ContactName =  name.Entity };
            // Add the new note to the list of notes and also save it in order to add text to it later
            contactToCreate = this.communicationByContact[comm.ContactName] = comm;

            // Prompt the user for what they want to say in the note           
            PromptDialog.Text(context, After_NumberPrompt, "What do you want to say in your number?");

        }

        private async Task After_NumberPrompt(IDialogContext context, IAwaitable<string> result)
        {
            // Set the text of the note
            contactToCreate.ContactAttribute = await result;

            await context.PostAsync($"Created contact **{this.contactToCreate.ContactName}** with \"{this.contactToCreate.ContactAttribute}\".");

            context.Wait(MessageReceived);
        }

    }
}