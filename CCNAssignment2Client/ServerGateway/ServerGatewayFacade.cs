using Entities;
using EntityLayer;
using UseCase;

namespace CCNAssignment2.ServerGateway;

public class ServerGatewayFacade : IDatabaseGatewayFacade
{
    public ServerGatewayFacade()
    {
    }

    public void AddBook(Book b)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Book = b,
            BookId = b.ID,
            Action = "AddBook"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void AddMember(Member m)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Member = m,
            MemberId = m.ID,
            Action = "AddMember"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void CreateLoan(Loan loan)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Loan = loan,
            LoanId = loan.ID,
            Action = "CreateLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void EndLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            BookId = bookId,
            Action = "EndLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void FindBook(int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            Action = "FindBook"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void FindLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            MemberId = memberId,
            Action = "FindLoan"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void FindMember(int memberId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            Action = "FindMember"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void GetAllBooks()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllBooks"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void GetAllMembers()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllMembers"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void GetCurrentLoans()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetCurrentLoans"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void InitialiseDatabase()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "InitialiseDatabase"
        };
        MyTcpClient.WriteToServer(messageDto);
    }

    public void RenewLoan(Loan loan)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "RenewLoan",
            Loan = loan
        };
        MyTcpClient.WriteToServer(messageDto);
    }
}