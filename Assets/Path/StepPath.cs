using System;
using System.Collections.Generic;
using PathCreation;
using UnityEditor;
using UnityEngine;

namespace App.Path
{
    public enum PointType
    {
        Current,
        NextActive,
        PreviousActive,
        Last,
        First
    }

    public class StepPath : MonoBehaviour
    {
        [SerializeField] protected PathCreator pathCreator = null;
        [SerializeField] protected List<PathPoint> _points = null;

        #region Path accsess

        public event Action<PathState> OnPathStateChange = null;
        public List<PathPoint> Points => _points;

        private int _currentPointIndex = 0;
        private int LastIndex => _points[_points.Count - 1].Index;
        private int ActivePointCount => _points.FindAll(p => p.IsActive).Count;
        private VertexPath VertexPath => pathCreator.path;

        public void SetCurrentPointOn(PointType type)
        {
            switch (type)
            {
                case PointType.Current:
                    _currentPointIndex = GetCurrentPoint().Index;
                    break;
                case PointType.NextActive:
                    _currentPointIndex = GetNextPoint().Index;
                    break;
                case PointType.PreviousActive:
                    _currentPointIndex = GetPreviousPoint().Index;
                    break;
                case PointType.Last:
                    _currentPointIndex = LastIndex;
                    break;
                case PointType.First:
                    _currentPointIndex = 0;
                    break;
            }

            OnPathStateChange?.Invoke(GetPathState());
        }

        public PathState GetPathState()
        {
            return new PathState(
                GetCurrentPoint(),
                GetNextPoint(),
                GetPreviousPoint(),
                VertexPath
            );
        }

        public float GetPathProgress()
        {
            if (_currentPointIndex == 0) return 0f;

            return Mathf.Clamp01((float)GetCurentPointActiveIndex() / (float)ActivePointCount);
        }

        private int GetCurentPointActiveIndex()
        {
            int activeIndex = 1;
            foreach (var point in _points.FindAll(p => p.IsActive))
            {
                if (point.Index == _currentPointIndex) return activeIndex;
                activeIndex++;
            }
            return activeIndex;
        }

        private PathPoint GetCurrentPoint()
        {
            return _points[_currentPointIndex];
        }

        private PathPoint GetNextPoint()
        {
            for (int i = _currentPointIndex; i < LastIndex;)
            {
                i++;
                if (_points[i].IsActive)
                {
                    return _points[i];
                }
            }

            return _points[LastIndex];
        }

        private PathPoint GetPreviousPoint()
        {
            for (int i = _currentPointIndex; i > 0; i--)
            {
                if (_points[i].IsActive)
                {
                    return _points[i];
                }
            }

            return _points[0];
        }

        #endregion

        #region Path settings

#if UNITY_EDITOR
        public void CalculatePathPointsInEditor()
        {
            _points = new List<PathPoint>();
            int index = 0;

            for (int i = 0; i < pathCreator.bezierPath.NumPoints; i++)
            {
                if (i == 0 || i == pathCreator.bezierPath.NumPoints - 1 || i % 3 == 0) // Get only anchor positions
                {
                    Vector3 position = pathCreator.bezierPath.GetPoint(i);
                    Debug.Log(index);
                    float distance = GetPointDistanceOnPath(position, 0.001f);
                    Quaternion rotation = pathCreator.path.GetRotationAtDistance(distance, EndOfPathInstruction.Stop);

                    _points.Add(new PathPoint(index, position, rotation, distance));
                    index++;
                }
            }

            Debug.Log("REFRESH!");
        }

        private float GetPointDistanceOnPath(Vector3 point, float accuracy)
        {
            float resultDistance = 0;

            Vector3 RoundVector(Vector3 origin, int numberAfterComma)
            {
                return new Vector3((float)Math.Round(origin.x, numberAfterComma),
                    (float)Math.Round(origin.y, numberAfterComma),
                    (float)Math.Round(origin.z, numberAfterComma));
            }

            while (resultDistance < pathCreator.path.length)
            {
                Vector3 distancePoint = pathCreator.path.GetPointAtDistance(resultDistance);

                if (RoundVector(distancePoint, 0) == RoundVector(point, 0))
                {
                    return resultDistance;
                }

                if (Mathf.Abs(Vector3.Distance(distancePoint, point)) < 0.25f)
                {
                    return resultDistance;
                }

                resultDistance += accuracy;
            }
            Debug.LogError("LAST");
            return pathCreator.path.length;
        }

        private void OnDrawGizmos()
        {
            if (_points == null || _points.Count == 0) return;

            foreach (var point in _points)
            {
                if (point.IsActive)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point.Position, 1);
                }

                Handles.Label(point.Position, (point.Index).ToString());
            }
        }
#endif

        #endregion
    }

    public struct PathState
    {
        public bool IsCurrentLast => Current.Index == Next.Index;
        public bool IsCurrentFirst => Current.Index == Previous.Index;

        public readonly PathPoint Current;
        public readonly PathPoint Next;
        public readonly PathPoint Previous;
        public readonly VertexPath VertexPath;

        public PathState(PathPoint current, PathPoint next, PathPoint previous,
            VertexPath vertexPath)
        {
            Current = current;
            Next = next;
            Previous = previous;
            VertexPath = vertexPath;
        }
    }
}