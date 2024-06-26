using UnityEngine;

namespace Nextension
{
    public static class AutoTransformUtil
    {
        public static AutoTransformHandle autoMove(this Transform target, Vector3 speed, bool isLocalSpace = true)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoMove, speed, isLocalSpace);
        }
        public static AutoTransformHandle autoRotate(this Transform target, Vector3 speed, bool isLocalSpace = true)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, speed, isLocalSpace);
        }
        public static AutoTransformHandle autoRotateX(this Transform target, float degreesPerSecond, bool isLocalSpace = true)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(degreesPerSecond, 0, 0), isLocalSpace);
        }
        public static AutoTransformHandle autoRotateY(this Transform target, float degreesPerSecond, bool isLocalSpace = true)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(0, degreesPerSecond, 0), isLocalSpace);
        }
        public static AutoTransformHandle autoRotateZ(this Transform target, float degreesPerSecond, bool isLocalSpace = true)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(0, 0, degreesPerSecond), isLocalSpace);
        }
    }
}
