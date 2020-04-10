using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyVaultTest.Helpers
{
    public class SecurityGroups
    {
        public string CoalitionReadOnlyObjectLabel { get; set; }
        public string CoalitionReadOnlyObjectId { get; set; }

        public string CoalitionEditObjectLabel { get; set; }
        public string CoalitionEditObjectId { get; set; }

        public string CoalitionAdminObjectLabel { get; set; }
        public string CoalitionAdminObjectId { get; set; }
    }
}
