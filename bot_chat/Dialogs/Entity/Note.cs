using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bot_chat.Dialogs.Entity
{
    [Serializable]
    public sealed class Note : IEquatable<Note>
    {

        // CONSTANTS        
        // Name of note title entity
        public const string Entity_Note_Title = "Note.Title";
        // Default note title
        public const string DefaultNoteTitle = "default";

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
}