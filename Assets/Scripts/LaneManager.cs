using UnityEngine;

// Attach to the LaneManager empty GameObject.
// Defines the 3 lane X-positions the player moves between.
public class LaneManager : MonoBehaviour
{
    [Header("Lane Setup")]
    public float laneWidth = 2f;  // distance between lanes

    // 0 = left   1 = centre   2 = right
    public float GetLaneX(int laneIndex)
    {
        return (laneIndex - 1) * laneWidth;
    }

    // Draw lanes in the Editor so you can see them
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 3; i++)
        {
            float x = GetLaneX(i);
            Gizmos.DrawLine(new Vector3(x, 0, -50),
                            new Vector3(x, 0, 50));
        }
    }
}