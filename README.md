# Sistema de Gestión de Transmisiones - Core ????

Este es el núcleo (Core) del sistema transaccional para la gestión de un negocio de transmisiones de vehículos. Desarrollado como parte del proyecto para **INTEC**.

## ? Características Principales
- **Arquitectura Limpia:** Separación de capas (Core, Infraestructura, API).
- **Sistema de Logs:** Registro automático de auditoría y errores en SQL Server.
- **Seguridad:** Manejo de credenciales mediante plantillas de configuración.
- **Integración con Azure:** Base de datos SQL en la nube.

## ?? Configuración para el Equipo
Para que el proyecto corra en tu máquina local, sigue estos pasos:

1. Realiza un `git pull` de la rama `main`.
2. Busca el archivo `appsettings.Example.json` en el proyecto API.
3. Haz una copia y renómbrala a **`appsettings.json`**.
4. Edita el archivo y coloca el nombre del servidor y la **Contraseña de Azure** en la cadena de conexión.

## ??? Tecnologías
- .NET 8 / C#
- Entity Framework Core
- SQL Server (Azure)