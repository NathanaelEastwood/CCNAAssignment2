using Entities;
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
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Book = b,
            BookId = b.ID,
            Action = "AddBook"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.ResponseCode;
    }

    public int AddMember(Member m)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Member = m,
            MemberId = m.ID,
            Action = "AddMember"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.ResponseCode;
    }

    public int CreateLoan(Loan loan)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Loan = loan,
            LoanId = loan.ID,
            Action = "CreateLoan"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.ResponseCode;
    }

    public int EndLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            BookId = bookId,
            Action = "EndLoan"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.ResponseCode;
    }

    public Book FindBook(int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            Action = "FindBook"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Book.FirstOrDefault();
    }

    public Loan FindLoan(int memberId, int bookId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            BookId = bookId,
            MemberId = memberId,
            Action = "FindLoan"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Loan.FirstOrDefault();
    }

    public Member FindMember(int memberId)
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            MemberId = memberId,
            Action = "FindMember"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Member.FirstOrDefault();
    }

    public List<Book> GetAllBooks()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllBooks"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Book;
    }

    public List<Member> GetAllMembers()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetAllMembers"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Member;
    }

    public List<Loan> GetCurrentLoans()
    {
        ClientMessageDTO messageDto = new ClientMessageDTO
        {
            Action = "GetCurrentLoans"
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.Loan;
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
            Action = "RenewLoan",
            Loan = loan
        };
        ResponseMessageDTO response = MyTcpClient.WriteToServer(messageDto);
        return response.ResponseCode;
    }
}