using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Services.Notifications
{
    public class DesejoActionNotification : INotification
    {
        public Guid? ColecaoId { get; set; }

        public string Descricao { get; set; }

        public ActionNotification Action { get; set; }
    }
}
