using Dapper;
using Microsoft.Data.Sqlite;
using Cart.Api.Models;

namespace Cart.Api.Data
{
    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db");

        // ── GET CART BY USER ───────────────────────────────────────────────
        public async Task<Cart.Api.Models.Cart?> GetByUserIdAsync(Guid IdUsuario)
        {
            using var conn = CreateConnection();

            var cart = await conn.QuerySingleOrDefaultAsync<Cart.Api.Models.Cart>(
            """
            SELECT
                IdUsuario,
                FechaActualizacion
            FROM Cart
            WHERE IdUsuario = @IdUsuario
            """,
            new { IdUsuario = IdUsuario.ToString() });

            if (cart is null)
                return null;

            // Busca los ítems asociados a este IdUsuario específico
            var Items = await conn.QueryAsync<CartItem>(
            """
            SELECT
                IdProducto,
                Cantidad
            FROM CartItems
            WHERE IdUsuario = @IdUsuario
            """,
            new { IdUsuario = IdUsuario.ToString() });

            cart.Items = Items.ToArray();

            return cart;
        }

        // ── CREATE CART ────────────────────────────────────────────────────
        public async Task<Cart.Api.Models.Cart> CreateAsync(Guid IdUsuario)
        {
            // Chequeo previo para no duplicar ni lanzar excepciones si ya existe
            var existingCart = await GetByUserIdAsync(IdUsuario);
            if (existingCart is not null)
            {
                return existingCart;
            }

            using var conn = CreateConnection();

            await conn.ExecuteAsync(
            """
            INSERT INTO Cart
            (IdUsuario, FechaActualizacion)
            VALUES
            (@IdUsuario, datetime('now'))
            """,
            new
            {
                IdUsuario = IdUsuario.ToString()
            });

            return (await GetByUserIdAsync(IdUsuario))!;
        }

        // ── ADD ITEM ───────────────────────────────────────────────────────
        public async Task AddItemAsync(Guid IdUsuario, Guid IdProducto, int cantidad)
        {
            using var conn = CreateConnection();

            // Inserta el ítem vinculando el usuario y el producto correspondiente
            await conn.ExecuteAsync(
            """
            INSERT INTO CartItems
            (IdUsuario, IdProducto, Cantidad)
            VALUES
            (@IdUsuario, @IdProducto, @Cantidad)
            """,
            new
            {
                IdUsuario = IdUsuario.ToString(),
                IdProducto = IdProducto.ToString(),
                Cantidad = cantidad
            });

            // Actualiza la marca de tiempo del carrito principal
            await conn.ExecuteAsync(
            """
            UPDATE Cart
            SET FechaActualizacion = datetime('now')
            WHERE IdUsuario = @IdUsuario
            """,
            new{ IdUsuario = IdUsuario.ToString() });
        }

        // ── UPDATE ITEM ────────────────────────────────────────────────────
        public async Task<bool> UpdateItemAsync(Guid IdUsuario, Guid IdProducto, int cantidad)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync(
            """
            UPDATE CartItems
            SET Cantidad = @Cantidad
            WHERE IdUsuario = @IdUsuario
            AND IdProducto = @IdProducto
            """,
            new
            {
                IdUsuario = IdUsuario.ToString(),
                IdProducto = IdProducto.ToString(),
                Cantidad = cantidad
            });

            return rows > 0;
        }

        // ── REMOVE ITEM ────────────────────────────────────────────────────
        public async Task<bool> RemoveItemAsync(Guid IdUsuario, Guid IdProducto)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync(
            """
            DELETE FROM CartItems
            WHERE IdUsuario = @IdUsuario
            AND IdProducto = @IdProducto
            """,
            new
            {
                IdUsuario = IdUsuario.ToString(),
                IdProducto = IdProducto.ToString()
            });

            return rows > 0;
        }

        // ── CLEAR CART ─────────────────────────────────────────────────────
        public async Task<bool> ClearCartAsync(Guid IdUsuario)
        {
            using var conn = CreateConnection();

            // Limpia por completo las filas de ítems de este usuario
            var rows = await conn.ExecuteAsync(
            """
            DELETE FROM CartItems
            WHERE IdUsuario = @IdUsuario
            """,
            new
            {
                IdUsuario = IdUsuario.ToString()
            });

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