namespace Entities.State
{
    public class BookStateFactory
    {
        public static BookState Create(int numberOfRenewals)
        {
            if (numberOfRenewals == -1)
            {
                return BookState.Borrowed;
            }
            else
            {
                return BookState.Available;
            }
        }

        public static BookState Create(string state)
        {
            if (state == "Available")
            {
                return BookState.Available;
            }
            else
            { 
                return BookState.Borrowed;
            }
        }
    }
}
