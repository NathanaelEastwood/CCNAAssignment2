using CCNAssignment2.WPFPresenters;
using Controllers;
using CommandLineUI.Presenters;
using DTOs;
using UseCase;

namespace CommandLineUI.Commands
{
    class ViewAllBooksCommand : Command
    {

        public ViewAllBooksCommand()
        {
        }

        public UiViewData Execute()
        {
            ViewAllBooksController controller =
                new ViewAllBooksController(
                        new AllBooksPresenter());

            UiViewData data =
                controller.Execute();

            return data;
        }
    }
}
