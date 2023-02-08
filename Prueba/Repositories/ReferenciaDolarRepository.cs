using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using SQLitePCL;

namespace Prueba.Repositories
{
    public interface IReferenciaDolarRepository
    {
        Task<int> Crear(ReferenciaDolar referencia);
        Task<int> Editar(ReferenciaDolar referencia);
        Task<bool> ReferenciaDolarDiaActual();

        //Task<int> Eliminar(int id);
        bool ReferenciaDolarExists(int id);
    }

    public class ReferenciaDolarRepository: IReferenciaDolarRepository
    {
        private readonly PruebaContext _context;

        public ReferenciaDolarRepository(PruebaContext context)
        {
            _context = context;
        }

        public async Task<int> Crear(ReferenciaDolar referenciaDolar)
        {
            _context.Add(referenciaDolar);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Editar(ReferenciaDolar referenciaDolar)
        {
            _context.Update(referenciaDolar);
            return await _context.SaveChangesAsync();
        }

        //public async Task<int> Eliminar(int id)
        //{

        //}

        public async Task<bool> ReferenciaDolarDiaActual()
        {
            var referencia = await _context.ReferenciaDolars.Where(c => c.Fecha == DateTime.Now).ToListAsync();
            if (referencia != null && referencia.Any())
            {
                return true;
            }
            return false;
        }

        public bool ReferenciaDolarExists(int id)
        {
            return (_context.ReferenciaDolars?.Any(e => e.IdReferencia == id)).GetValueOrDefault();
        }


    }
}
