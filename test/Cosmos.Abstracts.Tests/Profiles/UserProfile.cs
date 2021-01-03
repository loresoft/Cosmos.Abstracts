using System;
using DataGenerator;
using DataGenerator.Sources;

namespace Cosmos.Abstracts.Tests.Profiles
{
    public class UserProfile : MappingProfile<Tests.Models.User>
    {
        public override void Configure()
        {
            Property(p => p.Id).Value(_ => Guid.NewGuid().ToString("N"));
            Property(p => p.Name).DataSource<NameSource>();
            Property(p => p.Email).DataSource<EmailSource>();
        }
    }
}
