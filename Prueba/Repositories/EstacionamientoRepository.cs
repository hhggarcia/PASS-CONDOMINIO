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
        IList<PuestoE> PuestosEsta(int id);
    }
    public class EstacionamientoRepository: IEstacionamientoRepository
    {
        private readonly PruebaContext _context;

        public EstacionamientoRepository(PruebaContext context)
        {
            _context = context;
        }

        public async Task<int> Crear(Estacionamiento estacionamiento)
        {

        }

        public async Task<int> Editar(Estacionamiento estacionamiento)
        {

        }

        public async Task<int> Eliminar(int id)
        {

        }

        public IList<PuestoE> PuestosEsta(int id)
        {

        }

        public async Task<int> CrearPuestoEst(PuestoE puestoEst)
        {

        }

        public async Task<int> EditarPuestoEst(PuestoE puestoEst)
        {

        }

        public async Task<int> EliminarPuestoEst(int id)
        {

        }
    }
}
