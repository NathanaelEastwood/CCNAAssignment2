﻿using CCNAssignment2.CommandLineUI_Component.Commands;

namespace CommandLineUI.Commands
{
    class CommandFactory
    {
        private int _memberId;
        private int _bookId;

        public CommandFactory()
        {
        }

        public void SetBorrowParameters(int memberId, int bookId)
        {
            _memberId = memberId;
            _bookId = bookId;
        }

        public Command CreateCommand(int menuChoice)
        {
            switch (menuChoice)
            {
                case RequestUseCase.BORROW_BOOK:
                    return new BorrowBookCommand(_memberId, _bookId);
                
                case RequestUseCase.INITIALISE_DATABASE:
                    return new InitialiseDatabaseCommand();

                case RequestUseCase.RENEW_LOAN:
                    return new RenewLoanCommand(_memberId, _bookId);

                case RequestUseCase.RETURN_BOOK:
                    return new ReturnBookCommand();

                case RequestUseCase.VIEW_ALL_BOOKS:
                    return new ViewAllBooksCommand();

                case RequestUseCase.VIEW_ALL_MEMBERS:
                    return new ViewAllMembersCommand();

                case RequestUseCase.VIEW_CURRENT_LOANS:
                    return new ViewCurrentLoansCommand();

                default:
                    return new NullCommand();
            }
        }
    }
}
