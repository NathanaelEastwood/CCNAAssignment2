using Entities;
using Entities.State;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace DatabaseGateway
{
    class GetAllCurrentLoans : DatabaseSelector<List<Loan>>
    {

        public GetAllCurrentLoans()
        {
        }

        protected override string GetSQL()
        {
            return 
                "SELECT SDAM_Loan.ID, SDAM_Loan.MemberId, SDAM_Member.Name, " +
                        "SDAM_Loan.BookId, SDAM_Book.Author, SDAM_Book.Title, SDAM_Book.ISBN, " +
                        "SDAM_Loan.LoanDate, SDAM_Loan.DueDate, SDAM_Loan.NumberOfRenewals " +
                "FROM SDAM_Loan JOIN SDAM_Member ON SDAM_Loan.MemberId = SDAM_Member.Id " +
                               "JOIN SDAM_Book ON SDAM_Loan.BookId = SDAM_Book.Id " +
                "WHERE ReturnDate IS NULL";
        }

        // Gets all current loans
        protected override List<Loan> DoSelect(MySqlCommand command)
        {
            List<Loan> list = new List<Loan>();

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Member member = new MemberBuilder()
                            .WithId(reader.GetInt32(1))
                            .WithName(reader.GetString(2))
                            .Build();

                        Book book = new BookBuilder()
                            .WithId(reader.GetInt32(3))
                            .WithAuthor(reader.GetString(4))
                            .WithTitle(reader.GetString(5))
                            .WithISBN(reader.GetString(6))
                            .Build();

                        Loan loan = new LoanBuilder()
                            .WithID(reader.GetInt32(0))
                            .WithMember(member)
                            .WithBook(book)
                            .WithLoanDate(reader.GetDateTime(7))
                            .WithDueDate(reader.GetDateTime(8))
                            .WithNumberOfRenewals(reader.GetInt32(9))
                            .Build();

                        list.Add(loan);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: retrieval of loans failed", e);
            }

            return list;
        }
    }
}
