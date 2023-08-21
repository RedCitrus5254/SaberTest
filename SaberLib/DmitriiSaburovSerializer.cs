using MemoryPack;
using SerializerTests.Interfaces;
using SerializerTests.Nodes;

namespace SerializerTests.Implementations
{
    //Specify your class\file name and complete implementation.
    public class DmitriiSaburovSerializer : IListSerializer
    {
        //the constructor with no parameters is required and no other constructors can be used.
        public DmitriiSaburovSerializer()
        {
            //...
        }

        public Task<ListNode> DeepCopy(ListNode head)
        {
            var copyByListNode = new Dictionary<ListNode, ListNode>();
            var result = new ListNode();
            copyByListNode.Add(head, result);

            var current = result;
            ListNode? previous = null;

            while (head != null)
            {
                current.Data = head.Data;
                current.Previous = previous;

                if (head.Next != null)
                {
                    if (copyByListNode.TryGetValue(head.Next, out var next))
                    {
                        current.Next = next;
                    }
                    else
                    {
                        current.Next = new ListNode();
                        copyByListNode.Add(head.Next, current.Next);
                    }
                }

                if (head.Random != null)
                {
                    if (copyByListNode.TryGetValue(head.Random, out var random))
                    {
                        current.Random = random;
                    }
                    else
                    {
                        current.Random = new ListNode();
                        copyByListNode.Add(head.Random, current.Random);
                    }
                }

                previous = current;
                current = current.Next;
                head = head.Next;
            }

            return Task.FromResult(result);
        }

        public Task<ListNode> Deserialize(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);

            return Map(GetFromStream(s));
        }

        public async Task Serialize(ListNode head, Stream s)
        {
            var serializableNodes = Map(head);

            foreach (var node in serializableNodes)
            {
                var bin = MemoryPackSerializer.Serialize(node);

                await s.WriteAsync(BitConverter.GetBytes(bin.Length));

                await s.WriteAsync(bin);
            }
        }

        private static IEnumerable<SerializableModels.ListNode> Map(
            ListNode listNode)
        {
            var idByListNode = new Dictionary<ListNode, string>();

            string? curId = Guid.NewGuid().ToString();

            idByListNode.Add(listNode, curId);

            while (listNode != null)
            {
                string? nextId = null;
                if (listNode.Next != null)
                {
                    if (idByListNode.TryGetValue(listNode.Next, out var nextNodeId))
                    {
                        nextId = nextNodeId;
                    }
                    else
                    {
                        nextId = Guid.NewGuid().ToString();
                        idByListNode.TryAdd(listNode.Next, nextId);
                    }
                }

                string? randomId = null;
                if (listNode.Random != null)
                {
                    if (idByListNode.TryGetValue(listNode.Random, out var randomNodeId))
                    {
                        randomId = randomNodeId;
                    }
                    else
                    {
                        randomId = Guid.NewGuid().ToString();
                        idByListNode.TryAdd(listNode.Random, randomId);
                    }
                }

                yield return new SerializableModels.ListNode(
                    id: curId!,
                    randomId: randomId,
                    data: listNode.Data);

                curId = nextId;
                listNode = listNode.Next;
            }
        }

        private static async IAsyncEnumerable<SerializableModels.ListNode> GetFromStream(
            Stream stream)
        {
            while (stream.Position < stream.Length)
            {
                var countBuffer = new byte[4];
                await stream.ReadAsync(countBuffer, 0, 4);

                var bytesToRead = BitConverter.ToInt32(countBuffer, 0);

                var buffer = new byte[bytesToRead];
                await stream.ReadAsync(buffer, 0, bytesToRead);

                yield return MemoryPackSerializer.Deserialize<SerializableModels.ListNode>(buffer)!;
            }
        }

        private static async Task<ListNode> Map(
            IAsyncEnumerable<SerializableModels.ListNode> serializableNodes)
        {
            var listNodeById = new Dictionary<string, ListNode>();

            ListNode? prev = null;

            var head = new ListNode();

            var current = head;

            await foreach (var node in serializableNodes)
            {
                if (listNodeById.TryGetValue(node.Id, out var existedNode))
                {
                    current = existedNode;
                }
                else
                {
                    listNodeById.Add(node.Id, current);
                }
                current.Data = node.Data;
                current.Previous = prev;

                if (prev != null)
                {
                    prev.Next = current;
                }

                if (node.RandomId != null)
                {
                    if (listNodeById.TryGetValue(node.RandomId, out var randomNode))
                    {
                        current.Random = randomNode;
                    }
                    else
                    {
                        current.Random = new ListNode();
                        listNodeById.Add(node.RandomId, current.Random);
                    }
                }

                prev = current;

                current = new ListNode();
            }

            return head;
        }
    }
}
