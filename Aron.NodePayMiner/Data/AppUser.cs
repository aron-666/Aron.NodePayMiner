using Microsoft.AspNetCore.Identity;

namespace Aron.NodePayMiner.Data
{
    public class AppUser : IdentityUser
    {
        public override string? NormalizedUserName { get => base.NormalizedUserName.ToUpper(); set => base.NormalizedUserName = value; }
    }
}
