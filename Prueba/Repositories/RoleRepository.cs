using Prueba.Areas.Identity.Data;
using Prueba.Core.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Prueba.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDBContext _context;

        public RoleRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public ICollection<IdentityRole> GetRoles()
        {
            return _context.Roles.ToList();
        }
    }
}