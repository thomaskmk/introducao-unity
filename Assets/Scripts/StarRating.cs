namespace ObstacleCourse
{
    // Only completed escapes are rated; defeat is handled by GameController.
    public static class StarRating
    {
        public static int ForTime(float elapsed, float threeStarSeconds, float twoStarSeconds)
        {
            if (elapsed <= threeStarSeconds) return 3;
            return elapsed <= twoStarSeconds ? 2 : 1;
        }
    }
}
