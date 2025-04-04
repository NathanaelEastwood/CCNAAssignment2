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
        ClientMessageDTO message = new ClientMessageDTO
        {
            Book = b,
            BookId = b.ID,
            Action = "AddBook"
        };
        MyTcpClient.WriteToServer(message);
        return 1;
    }

    public int AddMember(Member m)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Member = m,
            MemberId = m.ID,
            Action = "AddMember"
        };
        MyTcpClient.WriteToServer(messageDto);
        return 1;
    }

    public int CreateLoan(Loan loan)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Loan = loan,
            LoanId = loan.ID,
            Action = "CreateLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
        return 1;
    }

    public int EndLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            BookId = bookId,
            Action = "EndLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
        return 1;
    }

    public Book FindBook(int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            Action = "FindBook"
        };
        MyTcpClient.WriteToServer(messageDto);
        return null;
    }

    public Loan FindLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            MemberId = memberId,
            Action = "FindLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
        return null;
    }

    public Member FindMember(int memberId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            Action = "FindMember"
        };
        MyTcpClient.WriteToServer(messageDto);
        return new Member(1, "two");
    }

    public List<Book> GetAllBooks()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllMembers"
        };
        MyTcpClient.WriteToServer(messageDto);
        return new List<Book>();
    }

    public List<Member> GetAllMembers()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllMembers"
        };
        MyTcpClient.WriteToServer(messageDto);
        return new List<Member>();
    }

    public List<Loan> GetCurrentLoans()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetCurrentLoans"
        };
        MyTcpClient.WriteToServer(messageDto);
        return new List<Loan>();
    }

    public void InitialiseDatabase()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "InitialiseDatabase"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public int RenewLoan(Loan loan)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "RenewLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
        return 0;
    }
}