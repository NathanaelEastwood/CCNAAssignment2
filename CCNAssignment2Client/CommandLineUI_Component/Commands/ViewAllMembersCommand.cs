using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class ViewAllMembersCommand : Command
    {

        public ViewAllMembersCommand()
        {
        }

        public UiViewData Execute()
        {
            ViewAllMembersController controller =
                new ViewAllMembersController(
                        new AllMembersPresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
