using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class ReturnBookCommand : Command
    {

        public ReturnBookCommand()
        {
        }

        public UiViewData Execute()
        {
            ReturnBookController controller = 
                new ReturnBookController(
                    ConsoleReader.ReadInt("\nMember ID"),
                    ConsoleReader.ReadInt("Book ID"),
                    new MessagePresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
