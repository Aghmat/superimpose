using UnityEngine;

public abstract class CharacterMovement : MonoBehaviour
{
    public MovementParams movementParams = new MovementParams(7.5f, 10f);
    public bool isJumping;
    protected Vector3 movement = Vector3.zero;
    protected float speed;

    public abstract Vector3 Move(Vector3 movement);

    public abstract Vector3 Fall(Vector3 movement);

    public abstract Vector3 Jump(Vector3 movement);

    public abstract bool IsGrounded { get; }
    public abstract Vector3 Velocity { get; }
}