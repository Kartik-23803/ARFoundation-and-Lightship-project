using UnityEngine;

public class PickupType : MonoBehaviour
{
    public enum ObjectType
    {
        Coin,           // Basic currency pickup
        SpeedBoost,     // Increases player speed
        Shield,         // Provides temporary invincibility
        DoublePoints,   // Multiplies point gain
        Magnet,         // Attracts nearby collectibles
        ExtraLife,      // Adds an extra life
        Obstacle,       // Harmful object that damages player
        JumpBoost,      // Increases jump height
        TimeFreeze,     // Slows down game time
        Multiplier      // Increases score multiplier
    }

    [Header("Pickup Configuration")]
    public ObjectType pickupType;

    [Header("Pickup Details")]
    public int value = 1;           // Generic value for pickup (coins, points, etc.)
    public float duration = 5f;     // Duration for temporary effects
}