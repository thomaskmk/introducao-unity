namespace ObstacleCourse
{
    // Shared by the live score and final rating; only a successful escape awards the stars.
    public static class StarRating
    {
        public static int ForTime(float elapsed, float threeStarSeconds, float twoStarSeconds)
        {
            if (elapsed <= threeStarSeconds) return 3;
            return elapsed <= twoStarSeconds ? 2 : 1;
        }
    }
}
