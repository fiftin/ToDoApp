using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class TodoUpdateData
    {
        public bool? IsDone { get; set; }
        public string? Text { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<List<Todo>> Get() => await _todoService.GetAsync();

        [HttpPost]
        public async Task<Todo> Create(Todo todo)
        {
            return await _todoService.CreateAsync(todo);
        }

        [HttpDelete("{id}")]
        public async Task Remove(string id)
        {
            await _todoService.RemoveAsync(id);
        }

        [HttpPut("{id}")]
        public async Task Update(string id, TodoUpdateData todo)
        {
            if (todo.Text  != null)
            {
                await _todoService.SetTextAsync(id, todo.Text);
            }

            if (todo.IsDone != null)
            {
                await _todoService.SetIsDoneAsync(id, todo.IsDone == true);
            }
        }
    }
}