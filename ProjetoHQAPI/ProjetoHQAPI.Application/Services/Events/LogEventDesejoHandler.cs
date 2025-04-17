using MediatR;
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
        public Task Handle(ErrorNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"ERROR : '{notification.Error} \n {notification.Stack}'");
            }, cancellationToken);
        }

        Task INotificationHandler<DesejoActionNotification>.Handle(DesejoActionNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"Desejo {notification.ColecaoId} - foi {notification.Action.ToString().ToLower()} com sucesso !");
            }, cancellationToken);
        }
    }
}
