using System;
using System.Collections;
using App.Path;
using PathCreation;
using UnityEngine;

namespace App.Player
{
    public class StepPathMovement : MonoBehaviour
    {
        [Header("Moveable root")] [SerializeField]
        private Rigidbody _root = null;

        [Header("Move speed")] [SerializeField]
        private float _speed = 25f;

        public bool IsMoving { get; private set; } = false;
        public bool IsAbleToMove { get; private set; } = true;

        public event Action OnStartMove = null;
        public event Action OnGetToTarget = null;
        public event Action OnBreakMove = null;

        private IEnumerator Movement(PathPoint from, PathPoint to, VertexPath path)
        {
            IsMoving = true;
            float distance = from.Distance;
            OnStartMove?.Invoke();

            while (distance <= to.Distance)
            {
                distance += Time.deltaTime * _speed;

                Vector3 position = path.GetPointAtDistance(distance, EndOfPathInstruction.Stop);
                Quaternion rotation = path.GetRotationAtDistance(distance, EndOfPathInstruction.Stop);

                _root.MovePosition(position);
                _root.MoveRotation(new Quaternion(0, rotation.y, rotation.z, rotation.w).normalized);

                yield return new WaitForFixedUpdate();
            }

            OnGetToTarget?.Invoke();
            IsMoving = false;
        }

        public void PlacedOnPoint(PathPoint point) 
        {
            _root.transform.localPosition = point.Position;
            _root.transform.localRotation = point.Rotation;
        }

        public void SetMoveAbility(bool state)
        {
            IsAbleToMove = state;
        }

        public void Move(PathPoint from, PathPoint to, VertexPath path)
        {
            if (IsMoving || !IsAbleToMove) return;

            StartCoroutine(Movement(from, to, path));
        }

        public void StopMove()
        {
            StopAllCoroutines();
            OnBreakMove?.Invoke();
        }

    }
}