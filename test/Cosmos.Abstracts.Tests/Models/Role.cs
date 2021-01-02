using System.Collections.Generic;

namespace Cosmos.Abstracts.Tests.Models
{
    public class Role
    {
        public Role()
        {
            Claims = new List<Claim>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }


        public ICollection<Claim> Claims { get; set; }
    }

}
