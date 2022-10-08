using Backend.Models;
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

    public static class TodoSorter
    {
        class TodoWrapper
        {
            public Todo Todo { get; set; }
            public List<TodoWrapper> Items { get; } = new List<TodoWrapper>();

            public TodoWrapper(Todo todo)
            {
                this.Todo = todo;
            }

            public List<Todo> Flat()
            {
                var ret = new List<Todo>();
                ret.Add(Todo);

                foreach (var item in Items)
                {
                    ret.AddRange(item.Flat());
                }
                return ret;
            }
        }


        public static List<Todo> SortTodos(List<Todo> todos)
        {
            var todoWrappers = todos.Select(x => new TodoWrapper(x)).ToList();

            foreach (var wrapper in todoWrappers)
            {
                if (wrapper.Todo.ParentPath == null)
                {
                    continue;
                }

                var parent = todoWrappers.Single(x => x.Todo.Id == wrapper.Todo.ParentId);

                parent.Items.Add(wrapper);
            }

            var sortedTodos = new List<Todo>();

            foreach (var rootWrapper in todoWrappers.Where(x => x.Todo.ParentPath == null))
            {
                sortedTodos.AddRange(rootWrapper.Flat());
            }

            return sortedTodos;
        }

    }

    public class TodoService : ITodoService
    {
        private readonly IMongoCollection<Todo> _todos;
        private readonly MongoClient _client;
        public TodoService()
        {
            _client = new MongoClient("mongodb://localhost:27017");
            var db = _client.GetDatabase("TodoApp");
            _todos = db.GetCollection<Todo>("Todos");
        }

        public async Task<List<Todo>> GetAsync()
        {
            var todos = await _todos
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return TodoSorter.SortTodos(todos);
        }

        public async Task<Todo> CreateAsync(Todo todo)
        {
            var id = ObjectId.GenerateNewId().ToString();

            var parentOk = todo.ParentPath == null || 
                (await _todos.FindAsync(x => x.Id == todo.ParentId && x.ParentPath == todo.GrandparentPath)).Any();
            if (!parentOk)
            {
                throw new Exception("Not found");
            }

            todo.Id = id;
            todo.CreatedAt = DateTime.Now;
            
            using (var session = await _client.StartSessionAsync())
            {
                await _todos.InsertOneAsync(todo);

                if (todo.ParentPath != null)
                {
                    var parts = todo.ParentPath.Split("/");

                    await _todos.UpdateManyAsync(
                        session,
                        Builders<Todo>.Filter.In(
                            p => p.Id,
                    parts),
                        Builders<Todo>.Update.Set(p => p.IsDone, false));
                }
            }

            return todo;
        }

        public async Task RemoveAsync(string id)
        {
            var todo = await (await _todos.FindAsync(x => x.Id == id)).SingleAsync();

            using (var session = await _client.StartSessionAsync())
            {
                await _todos.DeleteManyAsync(
                    session,
                    Builders<Todo>.Filter.Regex(
                        p => p.ParentPath,
                        new BsonRegularExpression("^" + todo.Path)));

                await _todos.DeleteOneAsync(session, x => x.Id == id);
            }
        }

        public async Task SetIsDoneAsync(string id, bool isDone)
        {
            var todo = await (await _todos.FindAsync(x => x.Id == id)).SingleAsync();

            using (var session = await _client.StartSessionAsync())
            {
                await _todos.UpdateOneAsync(
                    session,
                    Builders<Todo>.Filter.Eq(p => p.Id, id),
                    Builders<Todo>.Update.Set(p => p.IsDone, isDone));

                if (isDone)
                {
                    var res = await _todos.UpdateManyAsync(
                        session,
                        Builders<Todo>.Filter.Regex(
                            p => p.ParentPath,
                            new BsonRegularExpression("^" + todo.Path)),
                        Builders<Todo>.Update.Set(p => p.IsDone, isDone));
                } else if (todo.ParentPath != null)
                {
                    var parts = todo.ParentPath.Split("/");
                    await _todos.UpdateManyAsync(
                        session,
                        Builders<Todo>.Filter.In(
                            p => p.Id,
                            parts),
                        Builders<Todo>.Update.Set(p => p.IsDone, isDone));
                }
            }
        }

        public async Task SetTextAsync(string id, string text)
        {
            await _todos.UpdateOneAsync(
                Builders<Todo>.Filter.Eq(p => p.Id, id),
                Builders<Todo>.Update.Set(p => p.Text, text));
        }

    }
}
