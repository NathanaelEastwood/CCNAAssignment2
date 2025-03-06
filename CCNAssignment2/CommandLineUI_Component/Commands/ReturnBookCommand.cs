using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class ReturnBookCommand : Command
    {
        private readonly int _memberId;
        private readonly int _bookId;

        public ReturnBookCommand(int memberId, int bookId)
        {
            _memberId = memberId;
            _bookId = bookId;
        }

        public UiViewData Execute()
        {
            ReturnBookController controller = 
                new ReturnBookController(
                    _memberId,
                    _bookId,
                    new MessagePresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
