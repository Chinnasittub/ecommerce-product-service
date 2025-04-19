using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using SharedLibrary.DTOs;
using Newtonsoft.Json;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var products = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (products == null) return NotFound();
            return products;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateProduct(int id, Product product)
        // {
        //     if (id != product.Id) return BadRequest();
        //     _context.Entry(product).State = EntityState.Modified;
        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID in the URL does not match the ID in the body.");
            }
        
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
        
            _context.Entry(product).State = EntityState.Modified;
        
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The product was modified or deleted by another process.");
            }
        
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // ตรวจสอบว่าผลิตภัณฑ์มีอยู่หรือไม่
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Product not found.");
        
            // เรียก OrderService เพื่อตรวจสอบว่ามีคำสั่งซื้อที่เกี่ยวข้องกับผลิตภัณฑ์นี้หรือไม่
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://order-service/api/Order/product/{id}");
        
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(orders) && orders != "[]")
                {
                    return BadRequest($"Product with ID {id} has existing orders and cannot be deleted.");
                }
            }
            else
            {
                return StatusCode(500, "Failed to validate product's orders.");
            }
        
            // ลบผลิตภัณฑ์
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted successfully.", Product = product });
        }

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteProduct(int id)
        // {
        //     var product = await _context.Products.FindAsync(id);
        //     if (product == null) return NotFound();

        //     _context.Products.Remove(product);
        //     await _context.SaveChangesAsync();
        //     return Ok(product);
        // }

        [HttpPost("reduce")]
        public async Task<IActionResult> ReduceStock([FromBody] StockUpdateDto dto)
        {

            if (dto.ProductId <= 0)
            {
                return BadRequest("ProductId is required.");
            }
        
            if (dto.Quantity <= 0)
            {
                return BadRequest("Quantity to reduce must be greater than zero.");
            }
        
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
        
                if (product == null)
                {
                    return NotFound($"Product ID {dto.ProductId} not found.");
                }
        
                if (product.Quantity < dto.Quantity)
                {
                    return BadRequest("Not enough stock to reduce. There are only " +
                                     $"{product.Quantity} items in stock.");
                }
        
                product.Quantity -= dto.Quantity;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
        
                return Ok(new { Message = "Stock reduced successfully.", Product = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while reducing stock: {ex.Message}");
            }
        }

    }
}

