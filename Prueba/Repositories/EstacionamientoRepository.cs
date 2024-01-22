using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Repositories
{
    public interface IEstacionamientoRepository
    {
        Task<int> Crear(Estacionamiento estacionamiento);
        Task<int> CrearPuestoEst(PuestoE puestoEst);
        Task<int> Editar(Estacionamiento estacionamiento);
        Task<int> EditarPuestoEst(PuestoE puestoEst);
        Task<int> Eliminar(int id);
        Task<int> EliminarPuestoEst(int id);
        bool EstacionamientoExists(int id);
        bool PuestoEExists(int id);
        Task<IList<PuestoE>> PuestosEsta(int id);
    }
    public class EstacionamientoRepository: IEstacionamientoRepository
    {
        private readonly NuevaAppContext _context;

        public EstacionamientoRepository(NuevaAppContext context)
        {
            _context = context;
        }

        public async Task<int> Crear(Estacionamiento estacionamiento)
        {
            _context.Add(estacionamiento);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Editar(Estacionamiento estacionamiento)
        {
            _context.Update(estacionamiento);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Eliminar(int id)
        {
            var estacionamiento = await _context.Estacionamientos.FindAsync(id);

            // eliminar los puestos de estacionamiento relacionados

            if (estacionamiento != null)
            {
                _context.Estacionamientos.Remove(estacionamiento);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<IList<PuestoE>> PuestosEsta(int id)
        {
            var puestos = await _context.PuestoEs.Include(p => p.IdEstacionamientoNavigation)
                                                .Include(p => p.IdPropiedadNavigation)
                                                .Where(c => c.IdEstacionamiento == id)
                                                .ToListAsync();
            return puestos;
        }

        public async Task<int> CrearPuestoEst(PuestoE puestoEst)
        {
            _context.Add(puestoEst);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EditarPuestoEst(PuestoE puestoEst)
        {
            _context.Update(puestoEst);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EliminarPuestoEst(int id)
        {
            var puestoE = await _context.PuestoEs.FindAsync(id);
            if (puestoE != null)
            {
                _context.PuestoEs.Remove(puestoE);
            }

            return await _context.SaveChangesAsync();
        }

        public bool EstacionamientoExists(int id)
        {
            return (_context.Estacionamientos?.Any(e => e.IdEstacionamiento == id)).GetValueOrDefault();
        }

        public bool PuestoEExists(int id)
        {
            return (_context.PuestoEs?.Any(e => e.IdPuestoE == id)).GetValueOrDefault();
        }
    }
}
