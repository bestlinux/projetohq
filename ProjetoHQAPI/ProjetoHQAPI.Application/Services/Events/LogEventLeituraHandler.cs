using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Services.Events
{
    public class LogEventLeituraHandler : INotificationHandler<LeituraActionNotification>,
              INotificationHandler<ErrorNotification>
    {
        public Task Handle(ErrorNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"ERROR : '{notification.Error} \n {notification.Stack}'");
            }, cancellationToken);
        }

        Task INotificationHandler<LeituraActionNotification>.Handle(LeituraActionNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"Leitura {notification.LeituraId} - foi {notification.Action.ToString().ToLower()} com sucesso !");
            }, cancellationToken);
        }
    }
}
