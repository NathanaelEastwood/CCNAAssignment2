using CCNAssignment2.WPFPresenters;
using CommandLineUI.Presenters;
using DTOs;
using UseCase;

namespace CommandLineUI.Commands
{
    class NullCommand : Command
    {

        public NullCommand()
        {
        }

        public UiViewData Execute()
        {
            return new UiViewData([new MessageDTO("Choice not recognised")]);
        }
    }
}
