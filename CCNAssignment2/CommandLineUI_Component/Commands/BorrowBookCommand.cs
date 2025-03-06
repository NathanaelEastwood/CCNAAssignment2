using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;

namespace CommandLineUI.Commands
{
    class BorrowBookCommand : Command
    {

        private readonly int memberId;
        private readonly int bookId;
        
        public BorrowBookCommand(int memberId, int bookId)
        {
            this.memberId = memberId;
            this.bookId = bookId;
        }

        public UiViewData Execute()
        {
            BorrowBookController controller = 
                new BorrowBookController(
                    memberId,
                    bookId,
                    new MessagePresenter());

            UiViewData data = 
                controller.Execute();

            return data;
        }
    }
}
