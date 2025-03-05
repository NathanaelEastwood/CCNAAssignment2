using Entities;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseGateway
{
    class UpdateLoanReturnDate : DatabaseUpdater<Loan>
    {

        protected override string GetSQL()
        {
            return
                "UPDATE SDAM_Loan " +
                "SET returnDate = :returnDate " +
                "WHERE Id = :loanId";
        }

        protected override int DoUpdate(OracleCommand command, Loan loanToUpdate)
        {
            int numRowsAffected = 0;

            if (loanToUpdate != null)
            {
                try
                {
                    command.Prepare();
                    command.Parameters.Add(":returnDate", DateTime.Now);
                    command.Parameters.Add(":loanId", loanToUpdate.ID);
                    numRowsAffected = command.ExecuteNonQuery();

                    if (numRowsAffected != 1)
                    {
                        throw new Exception("ERROR: loan not updated");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }
            }
            return numRowsAffected;
        }
    }
}
