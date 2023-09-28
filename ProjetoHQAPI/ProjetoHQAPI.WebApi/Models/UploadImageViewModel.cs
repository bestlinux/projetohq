using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Models
{
    public class UploadImageViewModel
    {
        public IFormFile file { get; set; }
    }
}
