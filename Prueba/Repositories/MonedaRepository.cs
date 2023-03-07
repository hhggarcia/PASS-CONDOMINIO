using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Repositories
{
    public interface IMonedaRepository
    {
        Task<ICollection<MonedaCond>> BuscarPorCondominio(int idCondominio);
        Task<int> Crear(MonedaCond moneda);
        Task<int> Editar(MonedaCond moneda);
        Task<int> Eliminar(int id);
        bool MonedaCondExists(int id);
        Task<ICollection<MonedaCond>> MonedaPrincipal(int idCondominio);
    }
    public class MonedaRepository: IMonedaRepository
    {
        private readonly PruebaContext _context;

        public MonedaRepository(PruebaContext context)
        {
            _context = context;
        }

        public async Task<int> Crear(MonedaCond moneda)
        {
            _context.Add(moneda);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Editar(MonedaCond moneda)
        {
            _context.Update(moneda);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Eliminar(int id)
        {
            var monedaCond = await _context.MonedaConds.FindAsync(id);
            if (monedaCond != null)
            {
                _context.MonedaConds.Remove(monedaCond);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<ICollection<MonedaCond>> BuscarPorCondominio(int idCondominio)
        {
            var condominio = await _context.Condominios.FindAsync(idCondominio);

            if (condominio == null)
            {
                return new List<MonedaCond>();
            }

            var monedasCond = from moneda in _context.MonedaConds
                              where moneda.IdCondominio == condominio.IdCondominio
                              select moneda;

            return await monedasCond.ToListAsync();
        }

        public async Task<ICollection<MonedaCond>> MonedaPrincipal(int idCondominio)
        {
            var moneda = await _context.MonedaConds
                .Where(c => c.IdCondominio == idCondominio && c.Princinpal)
                .Include(d => d.IdMonedaNavigation)
                .ToListAsync();

            if (moneda == null || !moneda.Any())
            {
                return new List<MonedaCond>();
            }

            return moneda;            

        }

        public bool MonedaCondExists(int id)
        {
            return (_context.MonedaConds?.Any(e => e.IdMonedaCond == id)).GetValueOrDefault();
        }
    }
}
