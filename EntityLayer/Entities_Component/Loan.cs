﻿using System.Configuration;
using System.Text.Json.Serialization;

namespace Entities
{
    [Serializable]
    public class Loan : IEntity
    {
        public int ID { get; set; }
        public Member Member { get; set; }
        public Book Book { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime LoanDate { get; set; }
        public int NumberOfRenewals { get; set; }
        public DateTime ReturnDate { get; set; }

        [JsonConstructor]
        public Loan(int id, Member member, Book book, DateTime loanDate, DateTime dueDate)
        {
            ID = id;
            Member = member;
            Book = book;
            LoanDate = loanDate;
            DueDate = dueDate;
            ReturnDate = dueDate;
        }

        public Loan(int id, Member member, Book book, DateTime loanDate, DateTime dueDate, int numberOfRenewals)
        {
            ID = id;
            Member = member;
            Book = book;
            LoanDate = loanDate;
            DueDate = dueDate;
            NumberOfRenewals = numberOfRenewals;
        }
    }
}
