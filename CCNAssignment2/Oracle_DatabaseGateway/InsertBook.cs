using Entities;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseGateway
{
    class InsertBook : DatabaseInserter<Book>
    {

        protected override string GetSQL()
        {
            return
                "INSERT INTO SDAM_Book (ID, Author, Title, ISBN) " +
                "VALUES (SDAM_Book_Seq.nextval, :author, :title, :isbn)";
        }

        protected override int DoInsert(OracleCommand command, Book bookToInsert)
        {
            command.Prepare();
            command.Parameters.Add(":author", bookToInsert.Author);
            command.Parameters.Add(":title", bookToInsert.Title);
            command.Parameters.Add(":isbn", bookToInsert.ISBN);
            int numRowsAffected = command.ExecuteNonQuery();

            if (numRowsAffected != 1)
            {
                throw new Exception("ERROR: book not inserted");
            }
            return numRowsAffected;
        }
    }
}
