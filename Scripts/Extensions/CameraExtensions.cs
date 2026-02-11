using System;
using UnityEngine;
using Unity.Cinemachine;

namespace GAME.Extensions.Cinemachine
{
    public static class CameraExtensions
    {
        public static void Teleport(this CinemachineCamera camera, Transform transform)
        {
            camera.ForceCameraPosition(transform.position, transform.rotation);
            camera.PreviousStateIsValid = false;
        }

        public static void Teleport(this CinemachineCamera camera, Vector3 position)
        {
            camera.ForceCameraPosition(position, Quaternion.identity);
            camera.PreviousStateIsValid = false;
        }

        public static void SetTarget(this CinemachineCamera camera, Transform target)
        {
            camera.Target.TrackingTarget = target;
        }

        public static void SetDeadZone(this CinemachinePositionComposer composer, Vector2 size, bool enabled = true)
        {
            var composition = composer.Composition;
            composition.DeadZone.Enabled = enabled;
            composition.DeadZone.Size = size;
            composer.Composition = composition;
        }

        public static CinemachineCamera GetCameraByTag(this MonoBehaviour mono, string tag)
        {
            return Array.Find(mono.GetComponentsInChildren<CinemachineCamera>(true), c => c.CompareTag(tag));
        }
    }
}