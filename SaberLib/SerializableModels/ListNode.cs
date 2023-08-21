using MemoryPack;

namespace SerializerTests.SerializableModels;

[MemoryPackable]
public partial class ListNode
{
    public ListNode(
        string id,
        string? randomId,
        string? data)
    {
        RandomId = randomId;
        Id = id;
        Data = data;
    }

    public string Id { get; }

    public string? RandomId { get; }

    public string? Data { get; }
}

