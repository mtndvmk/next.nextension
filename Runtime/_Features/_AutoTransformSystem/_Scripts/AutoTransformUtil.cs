using UnityEngine;

namespace Nextension
{
    public static class AutoTransformUtil
    {
        public static AutoTransformHandle autoMove(this Transform target, Vector3 speed, bool isLocalSpace = true, bool isStopOnDisable = false)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoMove, speed, isLocalSpace, isStopOnDisable);
        }

        public static AutoTransformHandle autoRotate(this Transform target, Vector3 degreesPerSecond, bool isLocalSpace = true, bool isStopOnDisable = false)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, degreesPerSecond, isLocalSpace, isStopOnDisable);
        }

        public static AutoTransformHandle autoRotateX(this Transform target, float degreesPerSecond, bool isLocalSpace = true, bool isStopOnDisable = false)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(degreesPerSecond, 0, 0), isLocalSpace, isStopOnDisable);
        }

        public static AutoTransformHandle autoRotateY(this Transform target, float degreesPerSecond, bool isLocalSpace = true, bool isStopOnDisable = false)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(0, degreesPerSecond, 0), isLocalSpace, isStopOnDisable);
        }

        public static AutoTransformHandle autoRotateZ(this Transform target, float degreesPerSecond, bool isLocalSpace = true, bool isStopOnDisable = false)
        {
            return AutoTransformSystem.start(target, AutoTransformType.AutoRotate, new Unity.Mathematics.float3(0, 0, degreesPerSecond), isLocalSpace, isStopOnDisable);
        }
    }
}
