namespace Prueba.Core
{
    public class Constants
    {
        public static class Roles
        {
            public const string SuperAdmin= "SuperAdmin";
            public const string Administrador = "Administrador";
            public const string Propietario = "Propietario";
        }

        public static class Policies
        {
            public const string RequireSuperAdmin = "RequireSuperAdmin";
            public const string RequireAdmin = "RequireAdmin";
            public const string RequirePropietario = "RequirePropietario";
        }
    }
}

