using CCNAssignment2.WPFPresenters;
using DTOs;

namespace UseCase
{
    public abstract class AbstractController
    {

        protected AbstractPresenter presenter;

        public AbstractController(AbstractPresenter presenter)
        {
            this.presenter = presenter;
        }

        public UiViewData Execute()
        {
            presenter.DataToPresent = ExecuteUseCase();
            return presenter.ViewData;
        }

        public abstract IDto ExecuteUseCase();
    }
}
