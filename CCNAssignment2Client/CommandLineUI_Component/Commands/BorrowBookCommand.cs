using CCNAssignment2.WPFPresenters;
using CommandLineUI.Commands;
using CommandLineUI.Presenters;
using Controllers;

namespace CCNAssignment2.CommandLineUI_Component.Commands
{
    class BorrowBookCommand : Command
    {

        private readonly int _memberId;
        private readonly int _bookId;
        
        public BorrowBookCommand(int memberId, int bookId)
        {
            _memberId = memberId;
            _bookId = bookId;
        }

        public UiViewData Execute()
        {
            BorrowBookController controller = 
                new BorrowBookController(
                    _memberId,
                    _bookId,
                    new MessagePresenter());

            UiViewData data = 
                controller.Execute();

            return data;
        }
    }
}
