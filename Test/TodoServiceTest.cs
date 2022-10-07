using Backend.Models;
using Backend.Services;

namespace Test
{
    [TestClass]
    public class TodoSorterTest
    {
        [TestMethod]
        public void TestSortTodos()
        {
            var todos = new List<Todo>() {
                new Todo() { ParentPath = null, Id = "a" },
                new Todo() { ParentPath = "b", Id = "1" },
                new Todo() { ParentPath = "a", Id = "2" },
                new Todo() { ParentPath = "b", Id = "4" },
                new Todo() { ParentPath = null, Id = "b" },
            };

            var sortedTodos = TodoSorter.SortTodos(todos);

            Assert.AreEqual("a", sortedTodos[0].Id);
            Assert.AreEqual("2", sortedTodos[1].Id);
            Assert.AreEqual("b", sortedTodos[2].Id);
            Assert.AreEqual("1", sortedTodos[3].Id);
            Assert.AreEqual("4", sortedTodos[4].Id);
        }
    }
}