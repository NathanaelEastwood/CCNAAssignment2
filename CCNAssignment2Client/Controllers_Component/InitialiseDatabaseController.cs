using CCNAssignment2.ServerGateway;
using DatabaseGateway;
using DTOs;
using UseCase;

namespace Controllers
{
    public class InitialiseDatabaseController : AbstractController
    {
        public InitialiseDatabaseController(
            AbstractPresenter presenter) : base(presenter)
        {
        }

        public override IDto ExecuteUseCase()
        {
            return new InitialiseDatabase_UseCase(new ServerGatewayFacade()).Execute();
        }
    }
}
