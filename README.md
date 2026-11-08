
Trabajo Practico - Arquitectura de Microservicios (ECommerce)

Este repositorio contiene el desarrollo del Trabajo Practico del grupo, estructurado segun las pautas de la catedra. La solucion consiste en un sistema distribuido de 5 microservicios independientes que se comunican entre si de forma sincrona.

Estructura del repositorio

El codigo esta organizado de la siguiente manera:
• /src: Carpeta raiz que contiene los 5 proyectos (Cart.Api, Notificaciones, Orders, Productos.Api, Users.Api).
• /docs: Documentacion adicional del grupo, diagramas y capturas de las pruebas de logs.
• ECommerce.slnx: Archivo de solucion de Visual Studio para levantar todo el entorno.

Requisitos No Funcionales Implementados

• Tecnologias: .NET 8, Minimal APIs y Dapper para las consultas asincronas a las bases de datos de SQLite.
• Manejo Global de Errores: Uso de la interfaz IExceptionHandler para capturar excepciones y devolver respuestas estructuradas en formato JSON bajo el estandard de Problem Details (RFC 7807).
• Logs y Auditoria: Integracion de Serilog para escribir archivos de logs locales en formato JSON identado.
• Trazabilidad: Middleware propio que maneja el encabezado X-Correlation-ID para rastrear las solicitudes que viajan entre las distintas APIs.
• Monitoreo: Endpoints de salud configurados mediante AspNetCoreDiagnosticHealthChecks para revisar el estado del sistema en /health y el panel grafico en /health-ui.

Catalogo de Errores por Modulo


Usuarios (Users.Api)

• USR-001 (409 Conflict): El correo electronico ya se encuentra registrado.
• USR-002 (400 Bad Request): Datos invalidos o formato de mail incorrecto.
• USR-003 (401 Unauthorized): Credenciales incorrectas al intentar iniciar sesion.
• USR-004 (403 Forbidden): Cuenta bloqueada tras superar el maximo de 3 intentos fallidos consecutivos en la base de datos.
• USR-006 (500 Internal Server Error): Error interno inesperado del servidor.

Productos (Productos.Api)

• PRD-001 (404 Not Found): El producto solicitado no existe.
• PRD-002 (400 Bad Request): Formato invalido o campos obligatorios vacios.
• PRD-003 (409 Conflict): Ya existe un producto con ese nombre dentro de la misma categoria.
• PRD-005 (500 Internal Server Error): Error interno del servidor.

Carrito (Cart.Api)

• CRT-001 (404 Not Found): El carrito para el usuario solicitado no existe.
• CRT-002 (400 Bad Request): Identificadores de usuario o producto vacios o invalidos.
• CRT-003 (400 Bad Request): La cantidad de items ingresada o modificada debe ser mayor a cero o supera el stock del producto.
• CRT-004 (404 Not Found): El item solicitado no existe en el catalogo de productos o en el carrito.
• CRT-005 (409 Conflict): Conflicto en el estado del carrito o error de persistencia relacional.
• CRT-006 (500 Internal Server Error): Error interno del servidor.

Ordenes (Orders.Api)

• ORD-001 (404 Not Found): Orden de compra no encontrada.
• ORD-002 (400 Bad Request): Datos de la orden invalidos.
• ORD-003 (404 Not Found): Validacion remota: El usuario de la orden no existe en el sistema.
• ORD-004 (404 Not Found): Validacion remota: El producto de la orden no existe en el catalogo.
• ORD-005 (422 Unprocessable Entity): No hay stock suficiente del producto para procesar la orden.
• ORD-006 (409 Conflict): No se puede modificar el estado de una orden cancelada o entregada.
• ORD-007 (500 Internal Server Error): Error interno del servidor.

Notificaciones (Notificaciones.Api)

• NTF-001 (404 Not Found): Validacion remota: El usuario receptor de la alerta no existe en el sistema.
• NTF-002 (400 Bad Request): Campos obligatorios vacios o tipo de canal no reconocido (Solo se admite Email, Push o SMS).
• NTF-003 (404 Not Found): No se encontraron notificaciones registradas para el usuario especificado.
• NTF-004 (500 Internal Server Error): Error interno del servidor o falla general en la infraestructura de alertas.

Como correr el proyecto localmente

1. Clonar el repositorio.
2. Abrir el archivo ECommerce.slnx con Visual Studio.
3. Configurar la solucion para que inicien multiples proyectos en simultaneo (seleccionar las 5 APIs).
4. Ejecutar con F5. Cada microservicio levantara su propia interfaz de Swagger para hacer pruebas y consultar los estados de salud correspondientes.

