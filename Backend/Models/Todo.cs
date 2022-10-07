using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;

namespace Backend.Models
{
    public class Todo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Text { get; set; } = "";

        public bool IsDone { get; set; }

        public string? ParentPath { get; set; }

        public DateTime CreatedAt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { 
            get
            {
                return ParentPath?.Split("/").Last();
            } 
        }
        public string Path
        {
            get
            {
                if (ParentPath == null)
                {
                    return $"{Id}";
                }

                return string.Join("/", ParentPath, Id);
            }
        }

        public string? GrandparentPath
        {
            get
            {
                if (ParentPath == null)
                {
                    return null;
                }

                var parts = ParentPath.Split("/").SkipLast(1);

                if (parts.Count() == 0)
                {
                    return null;
                }

                return  string.Join("/", parts);
            }
        }
    }
}
