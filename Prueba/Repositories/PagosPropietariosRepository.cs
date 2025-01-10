using Prueba.Context;

namespace Prueba.Repositories
{
    public interface IPagosPropietariosRepository
    {
        Task<IList<int>> IdsCondominios(string IdUsuario);
    }
    public class PagosPropietariosRepository: IPagosPropietariosRepository
    {
        private readonly NuevaAppContext _context;

        public PagosPropietariosRepository(NuevaAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConfirmarPago()
        {

        }

        /// <summary>
        /// Busca todos los condominios a los cuales pertenece un usuario
        /// </summary>
        /// <param name="IdUsuario">Id del usuario</param>
        /// <returns>List<int> lista con los ids de los condominios encontrados</returns>
        public async Task<IList<int>> IdsCondominios(string IdUsuario)
        {
            var propiedades = from c in _context.Propiedads
                              where c.IdUsuario == IdUsuario
                              select c;

            List<int> listIdCondominios = new List<int>();

            if (propiedades != null && propiedades.Count() > 0)
            {
                foreach (var item in propiedades)
                {
                    //var inmueble = await _context.Inmuebles.FindAsync(item.Condmini);

                    listIdCondominios.Add(item.IdCondominio);

                }

                return listIdCondominios;
            }

            return listIdCondominios;
        }
    }
}
