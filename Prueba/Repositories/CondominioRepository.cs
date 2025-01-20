using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Repositories
{
    public interface ICondominioRepository
    {
        Task<IList<Propiedad>> GetPropiedadesCondominio(int id);
    }
    public class CondominioRepository: ICondominioRepository
    {
        private readonly NuevaAppContext _context;

        public CondominioRepository(NuevaAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// buscar todos los propietarios de una condominio
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public async Task<IList<AspNetUser>> GetUsersCondominio(int id)
        //{

        //}

        public async Task<IList<Propiedad>> GetPropiedadesCondominio(int id) => await _context.Propiedads.Include(c => c.IdUsuarioNavigation)
                .Where(c => c.IdCondominio == id)
                .OrderBy(c => c.Codigo)
                .ToListAsync();
    }
}
