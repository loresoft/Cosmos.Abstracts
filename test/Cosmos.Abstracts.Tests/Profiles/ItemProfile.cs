using System;
using Cosmos.Abstracts.Tests.Models;
using DataGenerator;
using DataGenerator.Sources;

namespace Cosmos.Abstracts.Tests.Profiles
{
    public class ItemProfile : MappingProfile<Tests.Models.Item>
    {
        public override void Configure()
        {
            Property(p => p.Id).Value(_ => Guid.NewGuid().ToString("N"));
            Property(p => p.Name).DataSource<NameSource>();
            Property(p => p.Description).DataSource<LoremIpsumSource>();
            Property(p => p.OwnerId).DataSource(Constants.Owners);
        }
    }
}
