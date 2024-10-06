using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSimba.Data;
using WebSimba.Models;

namespace WebSimba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imageFolderPath;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                return await _context.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCategories: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Category/2
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // POST: api/Category
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromForm] IFormCollection formCollection)
        {
            var category = new Category
            {
                Name = formCollection["Name"],
                ImagePath = formCollection["ImagePath"]
            };

            var imageFile = formCollection.Files.FirstOrDefault();

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(_imageFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                category.ImagePath = $"/images/{fileName}";
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // PUT: api/Category/2
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromForm] Category category, [FromForm] IFormFile imageFile)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(_imageFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                category.ImagePath = $"/images/{fileName}";
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Category/2
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Видалення зображення з файлової системи
            if (!string.IsNullOrEmpty(category.ImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
