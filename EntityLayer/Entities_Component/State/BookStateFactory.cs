namespace Entities.State
{
    public class BookStateFactory
    {
        public static BookState Create(int numberOfRenewals)
        {
            if (numberOfRenewals == -1)
            {
                return BookState.Available;
            }
            else
            {
                return BookState.Borrowed;
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
