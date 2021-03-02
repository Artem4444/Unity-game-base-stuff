using UnityEngine;

namespace App.Path
{
    [System.Serializable]
    public struct PathPoint
    {
        [SerializeField] bool _isActive;
        [HideInInspector] [SerializeField] Vector3 _position;
        [HideInInspector] [SerializeField] Quaternion _rotation;
        [HideInInspector] [SerializeField] int _index;
        [HideInInspector] [SerializeField] float _distanceOnPath;
        public bool IsActive => _isActive;
        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
        public float Distance => _distanceOnPath;
        public int Index => _index;

        public PathPoint(int index, Vector3 position,Quaternion rotation, float distance)
        {
            _index = index;
            _position = position;
            _rotation = rotation;
            _distanceOnPath = distance;
            _isActive = default;
        }
    }
}