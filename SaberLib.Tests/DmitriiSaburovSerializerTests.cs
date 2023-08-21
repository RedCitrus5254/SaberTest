using System.Diagnostics;
using FluentAssertions;
using SerializerTests.Nodes;
using Xunit;

namespace SaberLib.Tests;

public class DmitriiSaburovSerializerTests
{
    public class SerializationTests
    {
        [Fact]
        public async Task SerializeCorrectlyAsync()
        {
            var first = new ListNode
            {
                Data = "first",
            };
            var second = new ListNode()
            {
                Previous = first,
                Data = "second",
                Random = first,
            };
            var third = new ListNode
            {
                Previous = second,
                Data = "third",
                Random = first,
            };
            var fourth = new ListNode
            {
                Previous = third,
                Data = "fourth",
            };
            first.Next = second;
            first.Random = third;
            second.Next = third;
            third.Next = fourth;

            var sut = new SerializerTests.Implementations.DmitriiSaburovSerializer();

            var stream = new MemoryStream();

            await sut.Serialize(first, stream);

            var actual = await sut.Deserialize(stream);

            actual
                .Should()
                .BeEquivalentTo(
                    expectation: first,
                    config: options =>
                        options.Including(o => o.Data));

            var actualFirst = actual;
            var actualSecond = actualFirst.Next;
            var actualThird = actualSecond.Next;
            var actualFourth = actualThird.Next;

            actualFirst.Previous
                .Should()
                .BeNull();
            actualFourth.Previous
                .Should()
                .BeSameAs(actualThird);
            actualThird.Previous
                .Should()
                .BeSameAs(actualSecond);
            actualSecond.Previous
                .Should()
                .BeSameAs(actualFirst);

            actualFirst.Next
                .Should()
                .BeSameAs(actualSecond);
            actualSecond.Next
                .Should()
                .BeSameAs(actualThird);
            actualThird.Next
                .Should()
                .BeSameAs(actualFourth);
            actualFourth.Next
                .Should()
                .BeNull();

            actualFirst.Random
                .Should()
                .BeSameAs(actualThird);
            actualSecond.Random
                .Should()
                .BeSameAs(actualFirst);
            actualThird.Random
                .Should()
                .BeSameAs(actualFirst);
            actualFourth.Random
                .Should()
                .BeNull();
        }
    }

    public class DeepCopyTests
    {
        [Fact]
        public async Task CreatesDeepCopyCorrectlyAsync()
        {
            var first = new ListNode
            {
                Data = "first",
            };
            var second = new ListNode()
            {
                Previous = first,
                Data = "second",
                Random = first,
            };
            var third = new ListNode
            {
                Previous = second,
                Data = "third",
                Random = first,
            };
            var fourth = new ListNode
            {
                Previous = third,
                Data = "fourth",
            };
            first.Next = second;
            first.Random = third;
            second.Next = third;
            third.Next = fourth;

            var sut = new SerializerTests.Implementations.DmitriiSaburovSerializer();

            var actual = await sut.DeepCopy(first);

            actual
                .Should()
                .BeEquivalentTo(
                    expectation: first,
                    config: options =>
                        options.Including(o => o.Data));

            var actualFirst = actual;
            var actualSecond = actualFirst.Next;
            var actualThird = actualSecond.Next;
            var actualFourth = actualThird.Next;

            actualFirst.Previous
                .Should()
                .BeNull();
            actualFourth.Previous
                .Should()
                .BeSameAs(actualThird);
            actualThird.Previous
                .Should()
                .BeSameAs(actualSecond);
            actualSecond.Previous
                .Should()
                .BeSameAs(actualFirst);

            actualFirst.Next
                .Should()
                .BeSameAs(actualSecond);
            actualSecond.Next
                .Should()
                .BeSameAs(actualThird);
            actualThird.Next
                .Should()
                .BeSameAs(actualFourth);
            actualFourth.Next
                .Should()
                .BeNull();

            actualFirst.Random
                .Should()
                .BeSameAs(actualThird);
            actualSecond.Random
                .Should()
                .BeSameAs(actualFirst);
            actualThird.Random
                .Should()
                .BeSameAs(actualFirst);
            actualFourth.Random
                .Should()
                .BeNull();
        }
    }

    public class PerformanceTests
    {
        [Theory]
        [InlineData(1000000)]
        public async Task CheckPerformanceAsync(
            int elementsCount)
        {
            System.Console.WriteLine($"Elements count: {elementsCount}");
            var head = CreateNodes(elementsCount);

            var sut = new SerializerTests.Implementations.DmitriiSaburovSerializer();
            var stopwatch = Stopwatch.StartNew();

            await sut.DeepCopy(head);

            stopwatch.Stop();
            System.Console.WriteLine($"DeepCopy: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();

            var memoryStream = new MemoryStream();

            await sut.Serialize(head, memoryStream);

            stopwatch.Stop();
            System.Console.WriteLine($"Serialize: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();
            await sut.Deserialize(memoryStream);
            stopwatch.Stop();
            System.Console.WriteLine($"Deserialize: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Should().NotBeNull();
        }

        private static ListNode CreateNodes(
            int count)
        {
            var nodes = new List<ListNode>(count);
            var random = new Random();

            var head = new ListNode();

            var currentNode = head;
            ListNode? prevNode = null;

            for (var i = 0; i < count; i++)
            {
                nodes.Add(currentNode);
                var nextNode = new ListNode
                {
                    Data = i.ToString() + "data",
                };

                currentNode.Previous = prevNode!;
                currentNode.Next = nextNode;

                prevNode = currentNode;
                currentNode = nextNode;
            }

            currentNode = head;

            while (currentNode != null)
            {
                var randomIndex = random.Next(0, count);
                var randomNode = nodes[randomIndex];

                currentNode.Random = randomNode;

                currentNode = currentNode.Next;
            }

            return head;
        }
    }
}