using Entities;
using MySql.Data.MySqlClient;

namespace DatabaseGateway
{
    class FindBookById : DatabaseSelector<Book>
    {

        private int bookId;

        public FindBookById(int bookId)
        {
            this.bookId = bookId;
        }

        protected override string GetSQL()
        {
            return 
                "SELECT SDAM_Book.ID, Author, Title, ISBN, NumberOfRenewals " +
                "FROM SDAM_Book LEFT JOIN SDAM_Loan ON SDAM_Book.Id = SDAM_Loan.BookId AND SDAM_Loan.ReturnDate is null " +
                "WHERE SDAM_Book.id = @BookId";
        }

        protected override Book DoSelect(MySqlCommand command)
        {
            Book book = null;

            try
            {
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Prepare();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int numRenewals = reader.IsDBNull(4) ? -1 : reader.GetInt32(4);
                        book = new BookBuilder()
                            .WithId(reader.GetInt32(0))
                            .WithAuthor(reader.GetString(1))
                            .WithTitle(reader.GetString(2))
                            .WithISBN(reader.GetString(3))
                            .WithState(numRenewals)
                            .Build();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: retrieval of book failed", e);
            }
            finally
            {
                command.Dispose();
            }

            return book;
        }
    }
}
