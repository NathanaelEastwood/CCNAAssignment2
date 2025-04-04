using System.Text.Json.Serialization;

namespace Entities
{
    [Serializable]
    public class Book : IEntity
    {
        public int ID { get; }
        public string Author { get; }
        public string ISBN { get; }
        public string Title { get; }
        public BookState State { get; private set; }

        [JsonConstructor]
        public Book(int id, string author, string title, string isbn, BookState state)
        {
            ID = id;
            Author = author;
            Title = title;
            ISBN = isbn;
            State = state;
        }


        public override string ToString()
        {
            return Title;
        }

        public bool Borrow()
        {
            bool canBorrow = (State == BookState.Available);
            State = BookState.Borrowed;
            return canBorrow;
        }

        public bool Return()
        {
            bool canReturn = (State == BookState.Borrowed);
            State = BookState.Available;
            return canReturn;
        }
    }
}
