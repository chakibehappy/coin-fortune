using Spine;
using Spine.Unity;

public static class SpineHelper
{
    public static float GetAnimationDuration(SkeletonGraphic skeleton, string animationName)
    {
        return skeleton.Skeleton.Data.FindAnimation(animationName)?.Duration ?? 0;
    }

    public static float GetAnimationDuration(SkeletonAnimation skeleton, string animationName)
    {
        return skeleton.Skeleton.Data.FindAnimation(animationName)?.Duration ?? 0;
    }

    public static void PlayAnimation(SkeletonGraphic skeleton, string animationName, bool loop)
    {
        skeleton.AnimationState.SetAnimation(0, animationName, loop);
    }

    public static void PlayAnimation(SkeletonAnimation skeleton, string animationName, bool loop)
    {
        skeleton.AnimationState.SetAnimation(0, animationName, loop);
    }
    public static string GetCurrentAnimationName(SkeletonGraphic skeleton)
    {
        TrackEntry currentTrackEntry = skeleton.AnimationState.GetCurrent(0);
        if (currentTrackEntry != null)
        {
            return currentTrackEntry.Animation.Name;
        }
        else
        {
            return null;
        }
    }

    public static string GetCurrentAnimationName(SkeletonAnimation skeleton)
    {
        TrackEntry currentTrackEntry = skeleton.AnimationState.GetCurrent(0);
        if (currentTrackEntry != null)
        {
            return currentTrackEntry.Animation.Name;
        }
        else
        {
            return null;
        }
    }
}
