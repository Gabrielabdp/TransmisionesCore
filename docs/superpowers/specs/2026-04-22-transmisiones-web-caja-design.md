# Diseño: TransmisionesWeb — Portal del Cliente y Terminal de Caja

**Fecha:** 2026-04-22
**Proyecto:** TransmisionesSolution (INTEC)
**Rama base:** feature/integracion-fase3-endpointsNonCrud

---

## 1. Objetivo

Construir la interfaz web completa de Transmisiones MAG compuesta por dos secciones dentro de un único proyecto Blazor Server (`TransmisionesWeb`):

1. **Portal del Cliente** — el cliente se identifica por cédula, consulta sus órdenes y facturas, y puede recibir facturas por email.
2. **Terminal de Caja** — el empleado abre su sesión con su código, crea cotizaciones/órdenes, registra gastos, genera facturas e imprime el cuadre de caja al cierre del día.

Ambas secciones se conectan exclusivamente a `TransmisionesIntegracion`, que maneja el failover automático entre Azure SQL y SQLite local.

---

## 2. Arquitectura General

### 2.1 Proyectos involucrados

| Proyecto | Rol |
|---|---|
| `TransmisionesWeb` | Frontend Blazor Server (nuevo, se agrega a la solución) |
| `TransmisionesIntegracion` | Backend único — proxy offline-first con SQLite + sync |
| `TransmisionesAPI` | Backend Azure SQL — solo lo llama TransmisionesIntegracion |
| `TransmisionesCore` | Dominio — entidades, use cases, interfaces (sin cambios) |
| `TransmisionesInfraestructura` | Persistencia Azure — repositorios EF Core (sin cambios) |

### 2.2 Comunicación

```
Navegador del cliente
       ↓ HTTP (SignalR)
TransmisionesWeb (Blazor Server)
       ↓ HTTP (un solo HttpClient "ApiIntegracion")
TransmisionesIntegracion
       ├── Azure SQL disponible → TransmisionesAPI → Azure SQL
       └── Azure SQL caído    → SQLite local (caché)
                                    ↑ sync cada 30s (SincronizadorBackgroundService)
```

**Un solo `HttpClient` nombrado** (`ApiIntegracion`) registrado en `Program.cs` de TransmisionesWeb. Sin `ApiCore` directo.

### 2.3 Estado de sesión

`SessionStateService` (scoped — una instancia por conexión Blazor):

```csharp
public class SessionStateService
{
    // Portal cliente
    public int?    ClienteId   { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteEmail { get; set; }

    // Terminal de caja
    public int?    EmpleadoId  { get; set; }
    public string? EmpleadoNombre { get; set; }
    public string? EmpleadoRol { get; set; }
    public int?    CajaId      { get; set; }

    public bool EsClienteAutenticado => ClienteId.HasValue;
    public bool EsEmpleadoAutenticado => EmpleadoId.HasValue;
}
```

Sin JWT, sin cookies — apropiado para Blazor Server donde la sesión vive en el circuito SignalR.

### 2.4 Stack tecnológico

- .NET 8 / C# — Blazor Server con `@rendermode InteractiveServer`
- Bootstrap 5 + Bootstrap Icons (`bi-*`)
- MailKit — envío de email desde `TransmisionesIntegracion`
- Polly — reintentos en `HttpClient` (3 intentos, espera exponencial 1s/2s/4s)

### 2.5 Estructura de rutas

```
/                          → Página principal del portal (cliente)
/login                     → Identificación del cliente por cédula
/registro                  → Registro de cliente nuevo
/cliente/ordenes           → Órdenes activas del cliente autenticado
/cliente/facturas          → Historial de facturas del cliente
/cliente/factura/{id}      → Factura detallada — imprimir / enviar email

/empleado/apertura         → Login de empleado + apertura de caja
/empleado/caja             → Terminal de caja (cotización, productos, gastos)
/empleado/factura/{id}     → Factura generada — imprimir / enviar email
/empleado/cierre           → Cuadre de caja + reporte del día
```

---

## 3. Portal del Cliente

### 3.1 Autenticación

El cliente no tiene contraseña. Se identifica por número de cédula:

1. Ingresa cédula en `/login`.
2. Blazor llama `GET /api/integracion/clientes/documento/{cedula}`.
3. Si existe: guarda `{ClienteId, Nombre, Email}` en `SessionStateService` → redirige a `/cliente/ordenes`.
4. Si no existe: enlace a `/registro` para crear cuenta con nombre, teléfono y email.

Guardia de ruta: componente `<ClienteAuthGuard>` verifica `SessionStateService.EsClienteAutenticado` en cada página del portal. Si no está autenticado, redirige a `/login`.

### 3.2 Pantallas

**`/login`**
- Card con header dark, Bootstrap Icons (`bi-person-badge`).
- Campo cédula + botón "Acceder".
- Mensaje de error inline si no se encuentra la cédula.
- Enlace "Primera vez aquí → Regístrate".

**`/registro`**
- Formulario: Nombre completo, Cédula, Teléfono, Email, Provincia/Municipio/Sector (dropdowns desde caché de catálogos).
- Llama `POST /api/integracion/clientes`.
- Al registrarse exitosamente, inicia sesión y redirige a `/cliente/ordenes`.

**`/cliente/ordenes`**
- Lista de órdenes del cliente ordenadas por fecha descendente.
- Badge de estado por orden: `En proceso` (warning), `Lista` (info), `Entregada` (success).
- Cada orden muestra: vehículo (matrícula + modelo), servicios incluidos, fecha de ingreso.
- Botón "Ver Factura" en órdenes con estado `Entregada`.

**`/cliente/factura/{id}`**
- Layout de factura imprimible: cabecera con logo y datos de empresa, datos del cliente, tabla de productos/servicios con cantidad/precio, subtotal, ITBIS 18%, total en negrita.
- Botón "Imprimir" → `window.print()` via JS Interop.
- Botón "Enviar al correo" → `POST /api/integracion/facturas/{id}/enviar-email` → alerta de confirmación.

### 3.3 Flujo de datos

```
/cliente/ordenes
  → GET /api/integracion/ordenes?idCliente={id}
  → OrdenesCache (SQLite) o Azure según disponibilidad

/cliente/factura/{id}
  → GET /api/integracion/facturas/{id}        ← requiere Azure disponible
  → POST /api/integracion/facturas/{id}/enviar-email
```

> **Nota:** Las facturas no tienen caché local en SQLite (no existe `FacturaCache` en `IntegracionDbContext`). Si Azure no está disponible, la pantalla muestra "Factura disponible solo en línea" con botón de reintento. El resto del portal (órdenes, registro) sigue funcionando offline.

---

## 4. Terminal de Caja

### 4.1 Autenticación del empleado

1. En `/empleado/apertura` el empleado ingresa **código de usuario** (campo ya existente), **contraseña** (campo nuevo que se agrega al `.razor`) y el monto inicial.
2. Blazor llama `POST /api/integracion/autenticacion/login` con `{Usuario: codigo, Password: contraseña}`.
3. Si válido: la respuesta devuelve `{idEmpleado, rol, idSucursal, nombre}`. Se guarda en `SessionStateService`.
4. La caja se identifica por sucursal: se llama `GET /api/integracion/cajas?sucursal={idSucursal}` para obtener la caja activa de esa sucursal. Si hay más de una, se muestra un selector. El `CajaId` resultante se guarda en sesión.
5. Se llama `POST /api/integracion/cajas/{CajaId}/abrir` con el monto inicial → redirige a `/empleado/caja`.

Guardia de ruta: `<EmpleadoAuthGuard>` en todas las páginas `/empleado/*` excepto `/empleado/apertura`.

### 4.2 Terminal principal `/empleado/caja`

Layout de dos columnas:

**Columna izquierda — Búsqueda:**
- Campo de búsqueda con autocompletado que consulta `GET /api/integracion/productos?buscar={texto}` y `GET /api/integracion/servicios`.
- Al seleccionar un ítem: se agrega a la cotización con cantidad 1 y precio editable.
- Separador visual entre productos y servicios.

**Columna derecha — Cotización en curso:**
- Lista de ítems con cantidad, precio unitario, subtotal por línea.
- Totales: subtotal, ITBIS 18%, **total**.
- Botones de acción:
  - `Confirmar Orden` → crea orden + factura → redirige a `/empleado/factura/{id}`
  - `Registrar Gasto` → abre modal
  - `Limpiar` → limpia la cotización

**Modal "Registrar Gasto":**
- Campos: Concepto (texto), Monto (número).
- Llama `POST /api/integracion/cajas/{id}/gasto`.
- Se encola en `TransaccionesPendientes` si está offline.

### 4.3 Flujo de facturación

```
Confirmar Orden
  → POST /api/integracion/ordenes/procesar
      (crea orden + agrega productos/servicios + confirma + factura en un solo paso)
  → Responde con {idFactura}
  → Redirige a /empleado/factura/{idFactura}
```

Se usa el endpoint `POST /api/integracion/ordenes/procesar` ya implementado en `IntegracionOrdenesController` que hace el flujo completo con rollback compensatorio si falla.

### 4.4 Factura del empleado `/empleado/factura/{id}`

Misma presentación que la factura del cliente más:
- Botón "Imprimir" → `window.print()`.
- Botón "Enviar al correo del cliente" → `POST /api/integracion/facturas/{id}/enviar-email`.
- Botón "Nueva Operación" → limpia sesión de cotización y vuelve a `/empleado/caja`.

### 4.5 Cuadre de caja `/empleado/cierre`

- Llama `GET /api/integracion/cajas/{id}/estado-actual` para obtener saldo y movimientos.
- Muestra: ventas del día, gastos del día, efectivo esperado en caja, número de transacciones.
- Sección de resumen imprimible con fecha, nombre del empleado, sucursal.
- Botón "Imprimir Cuadre" → `window.print()`.
- Botón "Cerrar Caja" → `POST /api/integracion/cajas/{id}/cerrar` → redirige a `/empleado/apertura`.

### 4.6 Indicador offline

Componente `<EstadoConexion>` en el layout de `/empleado/*`:
- Consulta `GET /api/integracion/estado` cada 30 segundos.
- Muestra badge verde "En línea" o badge rojo "Modo Offline — X transacciones pendientes".
- No bloquea el uso de la caja en ningún caso.

---

## 5. Endpoints nuevos en TransmisionesIntegracion

| Método | Ruta | Propósito |
|---|---|---|
| `GET` | `/api/integracion/estado` | Retorna si Azure está alcanzable + count de transacciones pendientes |
| `GET` | `/api/integracion/facturas/{id}` | Lee factura desde Azure (no tiene cache local — requiere conexión) |
| `POST` | `/api/integracion/facturas/{id}/enviar-email` | Envía factura HTML al email del cliente vía MailKit |
| `POST` | `/api/integracion/cajas/{id}/gasto` | Registra un gasto — se encola si offline |

---

## 6. Resiliencia de Conexión

- TransmisionesIntegracion es el único punto de falla visible para el Blazor. Su lógica de failover (Azure → SQLite) es transparente.
- `HttpClient` en TransmisionesWeb tiene `Timeout` de 8s y política Polly: 3 reintentos con backoff exponencial (1s, 2s, 4s).
- Si TransmisionesIntegracion no responde en absoluto: mensaje de error claro en pantalla, sin stacktrace expuesto al usuario.
- Excepciones de dominio (`CajaYaAbiertaException`, `FacturaSinOrdenConfirmadaException`, etc.) se capturan en los componentes y se muestran como alertas Bootstrap.
- Formularios usan `EditForm` con `DataAnnotationsValidator` para validación antes de llamar la API.

---

## 7. Email de Facturas

- **Librería:** MailKit (NuGet) instalado en `TransmisionesIntegracion`.
- **Configuración SMTP** en `appsettings.json`:
  ```json
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Usuario": "",
    "Password": "",
    "NombreRemitente": "Transmisiones MAG"
  }
  ```
- **Template:** HTML generado en memoria con los datos de la factura (no requiere motor de plantillas externo).
- El endpoint `POST /api/integracion/facturas/{id}/enviar-email` obtiene la factura, construye el HTML y envía usando `MimeMessage` de MailKit.

---

## 8. Testing

**Testing manual (golden paths):**
- Portal: cédula → ver orden → ver factura → recibir email
- Caja online: login → apertura → cotización → factura → imprimir → cierre
- Caja offline: apagar TransmisionesAPI → operar → encender API → verificar sync

**Archivos `.http` nuevos** en `TransmisionesIntegracion.http`:
- `GET /api/integracion/estado`
- `GET /api/integracion/facturas/{id}`
- `POST /api/integracion/facturas/{id}/enviar-email`
- `POST /api/integracion/cajas/{id}/gasto`

---

## 9. Componentes Blazor — Lista Completa

### TransmisionesWeb/Components/Layout/
- `MainLayout.razor` — Layout base con navbar del portal cliente
- `EmpleadoLayout.razor` — Layout para sección de empleados con indicador offline

### TransmisionesWeb/Components/Shared/
- `SessionStateService.cs` — Servicio de sesión en memoria
- `EstadoConexion.razor` — Badge online/offline
- `ClienteAuthGuard.razor` — Guardia de autenticación cliente
- `EmpleadoAuthGuard.razor` — Guardia de autenticación empleado

### TransmisionesWeb/Components/Pages/
- `Home.razor` — Página principal del portal
- `Login.razor` — Identificación por cédula
- `Registro.razor` — Registro de cliente nuevo

### TransmisionesWeb/Components/Pages/Cliente/
- `Ordenes.razor`
- `Facturas.razor`
- `FacturaDetalle.razor`

### TransmisionesWeb/Components/Pages/Empleado/
- `AperturaCaja.razor` (ya existe — se conecta a la API)
- `TerminalCaja.razor`
- `FacturaEmpleado.razor`
- `CierreCaja.razor`
