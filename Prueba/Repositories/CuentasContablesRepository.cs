using Prueba.Context;

namespace Prueba.Repositories
{
    public interface ICuentasContablesRepository
    {

    }
    public class CuentasContablesRepository: ICuentasContablesRepository
    {
        private readonly PruebaContext _context;

        public CuentasContablesRepository(PruebaContext context)
        {
            _context = context;
        }
    }
}
