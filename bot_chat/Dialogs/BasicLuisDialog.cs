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
        private readonly Dictionary<string, Note> noteByTitle = new Dictionary<string, Note>();
        private readonly Dictionary<string, Communication> communicationByContact = new Dictionary<string, Communication>();

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

        [Serializable]
        public sealed class Note : IEquatable<Note>
        {
            public string Title { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return $"[{this.Title} : {this.Text}]";
            }

            public bool Equals(Note other)
            {
                return other != null
                    && this.Text == other.Text
                    && this.Title == other.Title;
            }

            public override bool Equals(object other)
            {
                return Equals(other as Note);
            }

            public override int GetHashCode()
            {
                return this.Title.GetHashCode();
            }
        }

        // CONSTANTS        
        // Name of note title entity
        public const string Entity_Note_Title = "Note.Title";
        public const string Entity_Communication_Name = "Communication.ContactName";
        public const string Entity_Name = "Name";
        
        // Default note title
        public const string DefaultCommunicationName = "default";
        public const string DefaultNoteTitle = "default";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance. {result.Intents[0].Score} {result.Intents[0].Intent}";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
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

        private Communication contact;
        private string currentName;
        [LuisIntent("Communication.AddContact")]
        public Task AddContactIntent(IDialogContext context, LuisResult result)
        {
            EntityRecommendation name;
            if (!result.TryFindEntity(Entity_Communication_Name, out name))
            {
                PromptDialog.Text(context, After_NamePrompt, "So, you want an add new contact, cool! What the name of contact ?");
            }
            else
            {
                var comm = new Communication() { ContactName = name.Entity };
                contact = this.communicationByContact[comm.ContactName] = comm;

                // Prompt the user for what they want to say in the note           
                PromptDialog.Text(context, After_NamePrompt, "Nome do contato");
            }

            return Task.CompletedTask;
        }

        [LuisIntent("Communication.FindContact")]
        public Task FindContactIntent(IDialogContext context, LuisResult result)
        {
            EntityRecommendation nameEntity;
            string name;
            if (result.TryFindEntity(Entity_Name, out nameEntity))
            {
                name = nameEntity.Entity;
                PromptDialog.Text(context, Find_NamePrompt, $"So, you want find {name}");
            }
            else
            {
                PromptDialog.Text(context, Find_NamePrompt, "Who you would like find?");
            }
            

            return Task.CompletedTask;
        }

        private async Task Find_NamePrompt(IDialogContext context, IAwaitable<string> result)
        {
            EntityRecommendation name;
            // Set the title (used for creation, deletion, and reading)
            currentName = await result;
            if (currentName != null)
            {
                contact = this.communicationByContact[currentName];
                if(contact != null)
                {
                    await context.PostAsync($"'Found contact, Name= **{this.contact.ContactName}** with \"{this.contact.ContactAttribute}\".");
                }
                else
                {
                    await context.PostAsync("Unfortunately, I unable found your contact! Probably it there isn't.");
                }
                context.Wait(MessageReceived);
            }
            else
            {
                PromptDialog.Text(context, Find_NamePrompt, "Sorry, but I cannot understand the name. Could you please repeat that?");
            }

        }

        private async Task After_NamePrompt(IDialogContext context, IAwaitable<string> result)
        {
            EntityRecommendation name;
            // Set the title (used for creation, deletion, and reading)
            currentName = await result;
            if (currentName != null)
            {
                name = new EntityRecommendation(type: Entity_Communication_Name) { Entity = currentName };
                var comm = new Communication() { ContactName = name.Entity };
                contact = this.communicationByContact[comm.ContactName] = comm;

                PromptDialog.Text(context, After_NumberPrompt, $"**{currentName}** is the name of contact. What's the number ?");
            }
            else
            {
                PromptDialog.Text(context, After_NamePrompt, "Sorry, but I cannot understand the name. Could you please repeat that?");
            }           

        }

        private async Task After_NumberPrompt(IDialogContext context, IAwaitable<string> result)
        {
            // Set the text of the note
            contact.ContactAttribute = await result;

            await context.PostAsync($"'Created contact **{this.contact.ContactName}** with \"{this.contact.ContactAttribute}\".");

            context.Wait(MessageReceived);
        }
                
    }
}