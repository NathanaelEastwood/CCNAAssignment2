using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class BorrowBookCommand : Command
    {

        public BorrowBookCommand()
        {
        }

        public UiViewData Execute()
        {
            BorrowBookController controller = 
                new BorrowBookController(
                    ConsoleReader.ReadInt("\nMember ID"),
                    ConsoleReader.ReadInt("Book ID"),
                    new MessagePresenter());

            UiViewData data = 
                controller.Execute();

            return data;
        }
    }
}
