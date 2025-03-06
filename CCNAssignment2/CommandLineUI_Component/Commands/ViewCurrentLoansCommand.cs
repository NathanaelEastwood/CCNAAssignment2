using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using UseCase;

namespace CommandLineUI.Commands
{
    class ViewCurrentLoansCommand : Command
    {

        public ViewCurrentLoansCommand()
        {
        }

        public UiViewData Execute()
        {
            ViewCurrentLoansController controller =
                new ViewCurrentLoansController(
                        new CurrentLoansPresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
