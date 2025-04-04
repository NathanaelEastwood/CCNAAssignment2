using System;
using System.Text.Json;
using Entities;
using EntityLayer;

namespace EntityLayer
{
    [Serializable]
    public class ClientMessageDTO
    {
        public string Action { get; set; }

        // Optional fields
        public int? BookId { get; set; }
        public int? MemberId { get; set; }
        public int? LoanId { get; set; }

        public Book? Book { get; set; }
        public Member? Member { get; set; }
        public Loan? Loan { get; set; }
    }
}