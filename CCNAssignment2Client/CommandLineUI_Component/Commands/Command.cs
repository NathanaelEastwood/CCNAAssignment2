using CCNAssignment2.WPFPresenters;
using UseCase;

namespace CommandLineUI.Commands
{
    interface Command
    {
        UiViewData Execute();
    }
}
