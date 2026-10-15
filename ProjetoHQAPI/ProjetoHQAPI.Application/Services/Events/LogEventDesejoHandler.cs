using MediatR;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Services.Events
{
    public class LogEventDesejoHandler : INotificationHandler<DesejoActionNotification>,
               INotificationHandler<ErrorNotification>
    {
        private readonly ILogger<LogEventDesejoHandler> _logger;

        public LogEventDesejoHandler(ILogger<LogEventDesejoHandler> logger)
        {
            _logger = logger;
        }
        public Task Handle(ErrorNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                _logger.LogError($"ERROR : '{notification.Error} \n {notification.Stack}'");
            }, cancellationToken);
        }

        Task INotificationHandler<DesejoActionNotification>.Handle(DesejoActionNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation($"Desejo {notification.DesejoId} - foi {notification.Action.ToString().ToLower()} com sucesso !");
            }, cancellationToken);
        }
    }
}
