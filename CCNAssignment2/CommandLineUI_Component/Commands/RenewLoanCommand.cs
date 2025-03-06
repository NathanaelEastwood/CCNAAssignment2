using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class RenewLoanCommand : Command
    {

        public RenewLoanCommand()
        {
        }

        public UiViewData Execute()
        {
            RenewLoanController controller = 
                new RenewLoanController(
                    ConsoleReader.ReadInt("\nMember ID"),
                    ConsoleReader.ReadInt("Book ID"),
                    new MessagePresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
