using Entities;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseGateway
{
    class InsertLoan : DatabaseInserter<Loan>
    {

        protected override string GetSQL()
        {
            return
                "INSERT INTO SDAM_Loan (ID, memberId, bookId, loanDate, dueDate, numberOfRenewals) " +
                "VALUES (SDAM_Loan_Seq.nextval, :mId, :bId, :loanDate, :dueDate, :numRenewals)";
        }

        protected override int DoInsert(OracleCommand command, Loan loanToInsert)
        {
            command.Prepare();
            command.Parameters.Add(":mId", loanToInsert.Member.ID);
            command.Parameters.Add(":bId", loanToInsert.Book.ID);
            command.Parameters.Add(":loanDate", loanToInsert.LoanDate);
            command.Parameters.Add(":dueDate", loanToInsert.DueDate);
            command.Parameters.Add(":numRenewals", loanToInsert.NumberOfRenewals);
            int numRowsAffected = command.ExecuteNonQuery();

            if (numRowsAffected != 1)
            {
                throw new Exception("ERROR: loan not inserted");
            }
            return numRowsAffected;
        }
    }
}
