using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;

namespace Service.ChangeBalanceGateway
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlTcpClient _myNoSqlClient;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlTcpClient myNoSqlClient)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClient = myNoSqlClient;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClient.Start();
            _logger.LogInformation("MyNoSqlTcpClient is started.");
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _myNoSqlClient.Stop();
            _logger.LogInformation("MyNoSqlTcpClient is stopped.");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
