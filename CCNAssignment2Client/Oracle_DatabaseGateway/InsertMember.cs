using Entities;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseGateway
{
    class InsertMember : DatabaseInserter<Member>
    {

        protected override string GetSQL()
        {
            return
                "INSERT INTO SDAM_Member (ID, Name) " +
                "VALUES (SDAM_Member_Seq.nextval, :name)";
        }

        protected override int DoInsert(OracleCommand command, Member memberToInsert)
        {
            command.Prepare();
            command.Parameters.Add(":name", memberToInsert.Name);
            int numRowsAffected = command.ExecuteNonQuery();

            if (numRowsAffected != 1)
            {
                throw new Exception("ERROR: member not inserted");
            }
            return numRowsAffected;
        }
    }
}
