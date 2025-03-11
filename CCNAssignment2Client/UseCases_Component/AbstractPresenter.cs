using CCNAssignment2.WPFPresenters;
using DTOs;

namespace UseCase
{
    public abstract class AbstractPresenter
    {

        public IDto DataToPresent { get; set; }

        public abstract UiViewData ViewData { get; }
    }
}
