using Entities;
using Entities.State;
using EntityLayer;
using UseCase;

namespace CCNAssignment2.ServerGateway;

public class ServerGatewayFacade : IDatabaseGatewayFacade
{
    public ServerGatewayFacade()
    {
    }

    public int AddBook(Book b)
    {
        
        MyTcpClient.WriteToServer(new Message("AddBook", 1, 2, 3));
        return 1;
    }

    public int AddMember(Member m)
    {
        MyTcpClient.WriteToServer(new Message("AddMember", 1, 2, 3));
        return 1;
    }

    public int CreateLoan(Loan loan)
    {
        MyTcpClient.WriteToServer(new Message("CreateLoan", 1, 2, 3));
        return 1;
    }

    public int EndLoan(int memberId, int bookId)
    {
        MyTcpClient.WriteToServer(new Message("EndLoan", 1, 2, 3));
        return 1;
    }

    public Book FindBook(int bookId)
    {
        MyTcpClient.WriteToServer(new Message("FindBook", 1, 2, 3));
        return null;
    }

    public Loan FindLoan(int memberId, int bookId)
    {
        MyTcpClient.WriteToServer(new Message("FindLoan", 1, 2, 3));
        return null;
    }

    public Member FindMember(int memberId)
    {
        MyTcpClient.WriteToServer(new Message("FindMember", 1, 2, 3));
        return new Member(1, "two");
    }

    public List<Book> GetAllBooks()
    {
        MyTcpClient.WriteToServer(new Message("GetAllBooks", 1, 2, 3));
        return new List<Book>();
    }

    public List<Member> GetAllMembers()
    {
        MyTcpClient.WriteToServer(new Message("GetAllMembers", 1, 2, 3));
        return new List<Member>();
    }

    public List<Loan> GetCurrentLoans()
    {
        MyTcpClient.WriteToServer(new Message("GetCurrentLoans", 1, 2, 3));
        return new List<Loan>();
    }

    public void InitialiseDatabase()
    {
        MyTcpClient.WriteToServer(new Message("InitialiseDatabase", 1, 2, 3));
    }

    public int RenewLoan(Loan loan)
    {
        MyTcpClient.WriteToServer(new Message("RenewLoan", 1, 2, 3));
        return 0;
    }
}