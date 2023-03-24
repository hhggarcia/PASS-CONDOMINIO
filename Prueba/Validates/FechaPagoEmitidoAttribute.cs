using System.ComponentModel.DataAnnotations;

namespace Prueba.Validates
{
    public class FechaPagoEmitidoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // value es la fecha del pago emitido 
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult("Debe Seleccionar una fecha");
            }

            // validar que sea la fecha en el mismo mes
            var fechaActual = DateTime.Today;

            var fecha = (DateTime)value;

            if (fecha.Month == fechaActual.Month)
            {
                return ValidationResult.Success;
            }


            return new ValidationResult("Solo se aceptan pagos del mes actual!");

        }
    }
}
