using Entities;
using System.Collections.Generic;

namespace UseCase
{

    public interface IDatabaseGatewayFacade
    {
        public void AddBook(Book b);

        public void AddMember(Member m);

        public void CreateLoan(Loan loan);

        public void EndLoan(int memberId, int bookId);

        public void FindBook(int bookId);

        public void FindLoan(int memberId, int bookId);

        public void FindMember(int memberId);

        public void GetAllBooks();

        public void GetAllMembers();

        public void GetCurrentLoans();

        public void InitialiseDatabase();

        public void RenewLoan(Loan loan);
    }
}
