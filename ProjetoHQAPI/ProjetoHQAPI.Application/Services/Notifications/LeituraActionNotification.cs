﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Services.Notifications
{
    public class LeituraActionNotification : INotification
    {
        public Guid? LeituraId { get; set; }

        public ActionNotification Action { get; set; }
    }
}
