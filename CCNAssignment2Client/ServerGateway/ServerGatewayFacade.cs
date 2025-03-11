using Entities;
using Entities.State;
using UseCase;

namespace CCNAssignment2.ServerGateway;

public class ServerGatewayFacade : IDatabaseGatewayFacade
{
    public ServerGatewayFacade()
    {
    }

    public int AddBook(Book b)
    {
        
        MyTcpClient.WriteToServer();
        return 1;
    }

    public int AddMember(Member m)
    {
        MyTcpClient.WriteToServer();
        return 1;
    }

    public int CreateLoan(Loan loan)
    {
        MyTcpClient.WriteToServer();
        return 1;
    }

    public int EndLoan(int memberId, int bookId)
    {
        MyTcpClient.WriteToServer();
        return 1;
    }

    public Book FindBook(int bookId)
    {
        MyTcpClient.WriteToServer();
        return null;
    }

    public Loan FindLoan(int memberId, int bookId)
    {
        MyTcpClient.WriteToServer();
        BookStateFactory bookStateFactory = new BookStateFactory();
        return null;
    }

    public Member FindMember(int memberId)
    {
        MyTcpClient.WriteToServer();
        return new Member(1, "two");
    }

    public List<Book> GetAllBooks()
    {
        MyTcpClient.WriteToServer();
        return new List<Book>();
    }

    public List<Member> GetAllMembers()
    {
        MyTcpClient.WriteToServer();
        return new List<Member>();
    }

    public List<Loan> GetCurrentLoans()
    {
        MyTcpClient.WriteToServer();
        return new List<Loan>();
    }

    public void InitialiseDatabase()
    {
        MyTcpClient.WriteToServer();
    }

    public int RenewLoan(Loan loan)
    {
        MyTcpClient.WriteToServer();
        return 0;
    }
}