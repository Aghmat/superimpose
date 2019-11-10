using System;
using UnityEngine;

[Serializable]
public struct MovementParams 
{
    [Header("Movement Settings")] 
    private readonly float _speed;
    public float speedMultiplier;
    public float speed => _speed * speedMultiplier;


    [Header("Jump Settings")]
    private readonly float _jumpHeight;
    public float jumpMultiplier;
    public float jumpHeight => _jumpHeight * jumpMultiplier;
    public float gravity;
    
    public MovementParams(float _speed , float _jumpHeight, float speedMultiplier = 1f, float jumpMultiplier = 1f, float gravity = -9.8f)
    {
        this._speed = _speed;
        this.speedMultiplier = speedMultiplier;
        this._jumpHeight = _jumpHeight;
        this.jumpMultiplier = jumpMultiplier;
        this.gravity = gravity;
    }
}
