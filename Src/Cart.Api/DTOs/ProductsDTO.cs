namespace Cart.Api.DTOs;

    public class ProductDto
    {
        public Guid IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int Stock { get; set; }
    }
