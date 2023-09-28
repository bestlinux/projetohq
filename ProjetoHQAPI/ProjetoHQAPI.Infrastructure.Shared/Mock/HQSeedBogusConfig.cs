using ProjetoHQApi.Domain.Entities;
using AutoBogus;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Shared.Mock
{
    public class HQSeedBogusConfig : AutoFaker<HQ>
    {
        public HQSeedBogusConfig()
        {
            Randomizer.Seed = new Random(8675309);
            var id = 1;
            RuleFor(m => m.Id, f => Guid.NewGuid());
            RuleFor(o => o.Titulo, f => f.Commerce.Product());
            RuleFor(o => o.Editora, f => f.Commerce.Department());
            //AUDITORIA
            RuleFor(o => o.Created, f => f.Date.Past(1));
            RuleFor(o => o.CreatedBy, f => f.Name.FullName());
            RuleFor(o => o.LastModified, f => f.Date.Recent(1));
            RuleFor(o => o.LastModifiedBy, f => f.Name.FullName());
        }
    }
}
