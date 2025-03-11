using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class InitialiseDatabaseCommand : Command
    {

        public InitialiseDatabaseCommand()
        {
        }

        public UiViewData Execute()
        {
            InitialiseDatabaseController controller =
                new InitialiseDatabaseController(
                        new MessagePresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
