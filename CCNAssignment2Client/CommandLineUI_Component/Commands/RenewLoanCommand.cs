using CCNAssignment2.WPFPresenters;
using CommandLineUI.Commands;
using CommandLineUI.Presenters;
using Controllers;

namespace CCNAssignment2.CommandLineUI_Component.Commands
{
    class RenewLoanCommand : Command
    {

        private readonly int MemberId;
        private readonly int BookId;
        public RenewLoanCommand(int memberId, int bookId)
        {
            MemberId = memberId;
            BookId = bookId;
        }

        public UiViewData Execute()
        {
            RenewLoanController controller = 
                new RenewLoanController(
                    MemberId,
                    BookId,
                    new MessagePresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}

