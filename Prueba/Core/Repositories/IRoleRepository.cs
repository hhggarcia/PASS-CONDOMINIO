using Prueba.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Prueba.Core.Repositories
{
    public interface IRoleRepository
    {
        ICollection<IdentityRole> GetRoles();
    }
}