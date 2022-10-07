using Backend.Models;
using DnsClient.Protocol;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace Backend.Services
{
    public interface ITodoService
    {
        Task<List<Todo>> GetAsync();
        Task<Todo> CreateAsync(Todo todo);
        Task RemoveAsync(string id);

        Task SetIsDoneAsync(string id, bool isDone);

        Task SetTextAsync(string id, string text);
    }

    public class TodoService : ITodoService
    {
        private readonly IMongoCollection<Todo> _todos;
        public TodoService()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var db = mongoClient.GetDatabase("TodoApp");
            _todos = db.GetCollection<Todo>("Todos");
        }

        public async Task<List<Todo>> GetAsync()
        {
            var todos = await _todos
                .Find(_ => true)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();

            foreach (var todo in todos)
            {
                if (todo.ParentId == null) {
                    continue;
                }

                var parent = todos.Single(x => x.Id == todo.ParentId);

                parent.Items.Add(todo);
            }

            return todos.FindAll(x => x.ParentId == null);
        }

        public async Task<Todo> CreateAsync(Todo todo)
        {
            var id = ObjectId.GenerateNewId().ToString();

            todo.Id = id;
            todo.CreatedAt = DateTime.Now;
            
            await _todos.InsertOneAsync(todo);

            return todo;
        }

        public async Task RemoveAsync(string id)
        {
            var todo = await (await _todos.FindAsync(x => x.Id == id)).SingleAsync();
            await _todos.DeleteManyAsync(x => x.ParentPath == todo.Path);
            await _todos.DeleteOneAsync(x => x.Id == id);
        }

        public async Task SetIsDoneAsync(string id, bool isDone)
        {
            await _todos.UpdateOneAsync(
                Builders<Todo>.Filter.Eq(p => p.Id, id),
                Builders<Todo>.Update.Set(p => p.IsDone, isDone));
        }

        public async Task SetTextAsync(string id, string text)
        {
            await _todos.UpdateOneAsync(
                Builders<Todo>.Filter.Eq(p => p.Id, id),
                Builders<Todo>.Update.Set(p => p.Text, text));
        }

    }
}
