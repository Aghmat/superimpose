using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    [SerializeField] private new Camera camera;

    [SerializeField] protected CharacterController characterController;

    public override bool IsGrounded => characterController.isGrounded;

    public override Vector3 Velocity =>
        characterController.transform.InverseTransformDirection(characterController.velocity) / movementParams.speed;

    private void Update()
    {
        if (!characterController.enabled || Time.timeScale == 0) return;
        movement = Move(movement); //XZ
        movement = Fall(Jump(movement)); //Y

        characterController.Move(movement * Time.deltaTime);
    }

    public override Vector3 Move(Vector3 movement)
    {
        var inputX = Input.GetAxis("Horizontal");
        var inputY = Input.GetAxis("Vertical");

        if (inputX != 0 || inputY != 0)
        {
            speed = movementParams.speed;
            var clamped = Vector2.ClampMagnitude(new Vector2(inputX, inputY) * speed, speed); //Clamps diagonal movement
            movement.x = clamped.x;
            movement.z = clamped.y;

            var t = Input.GetButton("Fire1") ? 0.5f : 0.2f;
            movement = Rotate(movement, t);
        }
        else
        {
            if (Input.GetButton("Fire1"))
                movement = Rotate(movement, 1);

            movement.x = 0;
            movement.z = 0;
        }

        return movement;
    }

    private Vector3 Rotate(Vector3 movement, float t)
    {
        //deals with rotating relative to the camera
        var cameraTransform = camera.transform;
        var tmp = cameraTransform.rotation;
        cameraTransform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0);
        movement = cameraTransform.TransformDirection(movement);
        cameraTransform.rotation = tmp;

        var characterControllerTransform = characterController.transform;
        characterControllerTransform.rotation =
            Quaternion.Slerp(
                characterControllerTransform.rotation,
                Quaternion.LookRotation(new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z)),
                t);

        return movement;
    }

    public override Vector3 Fall(Vector3 movement)
    {
        if (characterController.isGrounded && movement.y < 0)
            movement.y = movementParams.gravity;
        else
            movement.y += movementParams.gravity * Time.deltaTime;

        return movement;
    }

    public override Vector3 Jump(Vector3 movement)
    {
        if (!characterController.isGrounded || !Input.GetButtonDown("Jump")) return movement;
        movement.y = movementParams.jumpHeight;
        isJumping = true;

        return movement;
    }
}