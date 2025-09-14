using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Backend.Data;
using Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoDbContext _context;
        private readonly ILogger<TodoController> _logger;

        public TodoController(TodoDbContext context, ILogger<TodoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            _logger?.LogInformation("GET api/todo called");

            try
            {
                var todos = await _context.Todos.ToListAsync();
                _logger?.LogInformation("Retrieved {Count} todo items", todos?.Count ?? 0);
                return todos;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving todos from database");
                return StatusCode(500, "An error occurred while fetching todos.");
            }
        }

        // GET: api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return todo;
        }

        // POST: api/Todo
        [HttpPost]
        public async Task<ActionResult<Todo>> CreateTodo(Todo todo)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(todo.Title))
                {
                    return BadRequest("Title is required");
                }
                
                // Set creation timestamp
                todo.CreatedAt = DateTime.UtcNow;
                
                // Log the action
                Console.WriteLine($"Adding new Todo item with title: {todo.Title}");
                
                // Check database connection before trying to save
                if (!_context.Database.CanConnect())
                {
                    Console.WriteLine("Database connection failed before saving Todo item");
                    return StatusCode(500, "Database connection error. Please try again later.");
                }
                
                // Add to context
                _context.Todos.Add(todo);
                
                // Save to database with detailed error handling
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Todo item created successfully with ID: {todo.Id}");
                    return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database update error: {dbEx.Message}");
                    Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    return StatusCode(500, "Error saving to database. See server logs for details.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in CreateTodo: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An unexpected error occurred. See server logs for details.");
            }
        }

        // PUT: api/Todo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, Todo todo)
        {
            if (id != todo.Id)
            {
                return BadRequest();
            }

            _context.Entry(todo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
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

        // PUT: api/Todo/5/complete
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsCompleted = true;
            todo.CompletedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
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

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoExists(int id)
        {
            return _context.Todos.Any(e => e.Id == id);
        }
    }
}
