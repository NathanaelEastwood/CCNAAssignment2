using Entities;
using MySql.Data.MySqlClient;

namespace DatabaseGateway
{
    class FindMemberById : DatabaseSelector<Member>
    {

        private int memberId;

        public FindMemberById(int memberId)
        {
            this.memberId = memberId;
        }

        protected override string GetSQL()
        {
            return "SELECT ID, Name FROM SDAM_Member WHERE id = @MemberId";
        }

        protected override Member DoSelect(MySqlCommand command)
        {
            Member member = null;

            try
            {
                command.Parameters.AddWithValue("@MemberId", memberId);
                command.Prepare();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        member = new MemberBuilder()
                            .WithId(reader.GetInt32(0))
                            .WithName(reader.GetString(1))
                            .Build();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: retrieval of member failed", e);
            }

            return member;
        }
    }
}
