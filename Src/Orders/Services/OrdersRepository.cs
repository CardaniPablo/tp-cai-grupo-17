using Dapper;
using Microsoft.Data.Sqlite;
using Orders.Api.Models;


namespace Orders.Api.Data
{
    public class OrdersRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OrdersRepository> _logger;

        public OrdersRepository(IConfiguration config, ILogger<OrdersRepository> logger)
        {
            _config = config;
            _logger = logger;
        }

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GUARDAR NUEVA ORDEN (CREAR) ───────────────────────────────────────
        public async Task CreateAsync(Orders.Api.Models.Order nuevaOrden)
        {
            using var conn = CreateConnection();
            conn.Open();

            // Usamos una transacción para asegurar que si fallan los detalles, no se guarde la cabecera
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Insertar la Cabecera de la Orden
                await conn.ExecuteAsync("""
                    INSERT INTO Orders (idOrden, usuarioId, total, estado, fechaCreacion)
                    VALUES (@idOrden, @usuarioId, @total, @estado, @fechaCreacion)
                """, new
                {
                    idOrden = nuevaOrden.Id.ToString(),
                    usuarioId = nuevaOrden.UsuarioId.ToString(),
                    total = (double)nuevaOrden.Total,
                    estado = nuevaOrden.Estado,
                    fechaCreacion = nuevaOrden.FechaCreacion.ToString("o") // Formato ISO 8601 idóneo para SQLite
                }, transaction);

                // 2. Insertar cada renglón de detalle (OrderItems)
                foreach (var item in nuevaOrden.Items)
                {
                    await conn.ExecuteAsync("""
                        INSERT INTO OrderItems (idOrden, productoId, cantidad, precioUnitario)
                        VALUES (@idOrden, @productoId, @cantidad, @precioUnitario)
                    """, new
                    {
                        idOrden = nuevaOrden.Id.ToString(),
                        productoId = item.ProductoId.ToString(),
                        cantidad = item.Cantidad,
                        precioUnitario = (double)item.PrecioUnitario
                    }, transaction);
                }

                transaction.Commit();
                _logger.LogInformation("Orden {OrderId} y sus ítems guardados con éxito en la DB.", nuevaOrden.Id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error crítico al guardar la orden en la base de datos. Transacción revertida.");
                throw;
            }
        }

        // ── OBTENER DETALLE DE UNA ORDEN POR ID ──────────────────────────────
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            // 1. Buscamos la cabecera
            var rawOrder = await conn.QuerySingleOrDefaultAsync("""
                SELECT idOrden, usuarioId, total, estado, fechaCreacion
                FROM Orders
                WHERE idOrden = @idOrden
            """, new { idOrden = id.ToString() });

            if (rawOrder is null) return null;

            // 2. Buscamos los ítems asociados a esa cabecera
            var items = await conn.QueryAsync("""
                SELECT productoId, cantidad, precioUnitario
                FROM OrderItems
                WHERE idOrden = @idOrden
            """, new { idOrden = id.ToString() });

            // 3. Reconstruimos el Record inmutable mapeando los tipos de datos de SQLite
            return new Orders.Api.Models.Order
            {
                Id = Guid.Parse(rawOrder.idOrden),
                UsuarioId = Guid.Parse(rawOrder.usuarioId),
                Total = Convert.ToDecimal(rawOrder.total),
                Estado = rawOrder.estado,
                FechaCreacion = DateTime.Parse(rawOrder.fechaCreacion),
                Items = items.Select(i => new OrderItem
                {
                    ProductoId = Guid.Parse(i.productoId),
                    Cantidad = Convert.ToInt32(i.cantidad),
                    PrecioUnitario = Convert.ToDecimal(i.precioUnitario)
                }).ToArray()
            };
        }

        // ── LISTAR ÓRDENES POR USUARIO (Filtro ?usuarioId=) ───────────────────
        public async Task<IEnumerable<Orders.Api.Models.Order>> GetByUserIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();

            // 1. Usamos alias claros (AS) en el SELECT para que coincidan con las propiedades del bucle
            var rawOrders = await conn.QueryAsync("""
        SELECT 
            idOrden AS idOrden, 
            usuarioId AS usuarioId, 
            total AS total, 
            estado AS estado, 
            fechaCreacion AS fechaCreacion
        FROM Orders
        WHERE LOWER(usuarioId) = LOWER(@UsuarioId)
        ORDER BY fechaCreacion DESC
    """, new { UsuarioId = usuarioId.ToString() });

            var listaOrdenesCompletas = new List<Orders.Api.Models.Order>();

            if (rawOrders == null || !rawOrders.Any())
            {
                return listaOrdenesCompletas;
            }

            foreach (var rawOrder in rawOrders)
            {
                // 2. CORRECCIÓN: Los nombres de las propiedades dinámicas ahora coinciden exactamente con el SELECT de arriba
                if (rawOrder.idOrden == null || rawOrder.usuarioId == null) continue;

                var idOrden = Guid.Parse(rawOrder.idOrden.ToString());

                var items = await conn.QueryAsync("""
            SELECT productoId, cantidad, precioUnitario
            FROM OrderItems
            WHERE idOrden = @IdOrden
        """, new { IdOrden = idOrden.ToString() });

                listaOrdenesCompletas.Add(new Orders.Api.Models.Order
                {
                    Id = idOrden,
                    UsuarioId = Guid.Parse(rawOrder.usuarioId.ToString()),
                    Total = Convert.ToDecimal(rawOrder.total),
                    Estado = rawOrder.estado?.ToString() ?? "Pendiente",
                    FechaCreacion = DateTime.Parse(rawOrder.fechaCreacion.ToString()),
                    Items = items.Select(i => new OrderItem
                    {
                        ProductoId = Guid.Parse(i.productoId.ToString()),
                        Cantidad = Convert.ToInt32(i.cantidad),
                        PrecioUnitario = Convert.ToDecimal(i.precioUnitario)
                    }).ToArray()
                });
            }

            return listaOrdenesCompletas;
        }



        // ── ACTUALIZAR ESTADO DE LA ORDEN ────────────────────────────────────
        public async Task<bool> UpdateStatusAsync(Guid id, string nuevoEstado)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync("""
                UPDATE Orders
                SET Estado = @Estado
                WHERE idOrden = @idOrden
            """, new { Estado = nuevoEstado, idOrden = id.ToString() });

            return rows > 0;
        }
        public class SqliteGuidTypeHandler : Dapper.SqlMapper.TypeHandler<Guid>
        {
            public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
            {
                parameter.Value = value.ToString();
            }

            public override Guid Parse(object value)
            {
                // Esto le enseña a Dapper a transformar el TEXT de SQLite en el Guid real de C#
                return Guid.Parse((string)value);
            }
        }
    }
    
}