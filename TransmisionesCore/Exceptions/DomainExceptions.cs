namespace TransmisionesCore.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string producto)
        : base($"Stock insuficiente para: {producto}") { }
}

public class OrdenNoConfirmableException : DomainException
{
    public OrdenNoConfirmableException(int id)
        : base($"La orden {id} no puede confirmarse en su estado actual.") { }
}

public class CajaYaAbiertaException : DomainException
{
    public CajaYaAbiertaException(string codigo)
        : base($"La caja {codigo} ya está abierta.") { }
}

public class CajaCerradaException : DomainException
{
    public CajaCerradaException(string codigo)
        : base($"La caja {codigo} no está abierta.") { }
}

public class EntidadNoEncontradaException : DomainException
{
    public EntidadNoEncontradaException(string entidad, object id)
        : base($"{entidad} con ID {id} no encontrado(a).") { }
}

public class PrecioInvalidoException : DomainException
{
    public PrecioInvalidoException()
        : base("El precio de venta no puede ser menor al costo.") { }
}

public class FacturaSinOrdenConfirmadaException : DomainException
{
    public FacturaSinOrdenConfirmadaException()
        : base("Solo se puede facturar una orden confirmada.") { }
}
