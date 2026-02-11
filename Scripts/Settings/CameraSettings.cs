using Unity.Cinemachine;
using UnityEngine;

namespace GAME.Settings.Camera
{
    [CreateAssetMenu(fileName = "Camera Settings", menuName = "GAME/Settings/Camera")]
    public class CameraSettings : ScriptableObject
    {
        [field: SerializeField, Space(4)] public CinemachineBlenderSettings BlendSettings { get; private set; }
        [field: SerializeField, Space(6)] public Vector3 Damping { get; private set; } = Vector3.one;
        //[field: SerializeField][field: Space(2)] public Vector3 MinorDamping { get; private set; } = Vector3.one;
        //[field: SerializeField][field: Space(6)] public Vector2 DeadZoneSize { get; private set; } = new(0.2f, 0.3f);
        //[field: SerializeField][field: Space(6)] public bool UseDeadZone { get; private set; }
        [field: SerializeField, Space(6)] public Vector2 Offset { get; private set; } = Vector2.zero;
        [field: SerializeField, Range(0, 20), Space(6)] public float Distance { get; private set; } = 7;

        //[field: SerializeField] [field: Range(0, 20)][field: Space(2)] public float MoveDistance { get; private set; } = 7;
        //[field: SerializeField] [field: Range(0, 1)][field: Space] public float SpeedThreshold { get; private set; } = 0.2f;
        //[field: SerializeField] [field: Range(0, 10)][field: Space(2)] public float IdleCooldown { get; private set; } = 3;
    }
}