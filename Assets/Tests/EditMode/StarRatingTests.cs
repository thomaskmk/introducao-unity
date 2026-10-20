using NUnit.Framework;

namespace ObstacleCourse.Tests
{
    public sealed class StarRatingTests
    {
        [TestCase(24.99f, 3)]
        [TestCase(25f, 3)]
        [TestCase(25.01f, 2)]
        [TestCase(40f, 2)]
        [TestCase(40.01f, 1)]
        [TestCase(89.99f, 1)]
        public void CompletionRatingIncludesTheLimitAndDropsImmediatelyAfterIt(float elapsed, int expected)
        {
            Assert.AreEqual(expected, StarRating.ForTime(elapsed, 25, 40));
        }
    }
}
