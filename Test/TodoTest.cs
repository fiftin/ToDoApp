using Backend.Models;
using Backend.Services;

namespace Test
{
    [TestClass]
    public class TodoTest
    {
        [TestMethod]
        public void TestGrandparentPath()
        {
            var todo = new Todo()
            {
                Id = "test",
                ParentPath = "path/to/parent"
            };

            Assert.AreEqual("path/to", todo.GrandparentPath);

            todo.ParentPath = "parent";

            Assert.AreEqual(null, todo.GrandparentPath);

        }
    }
}