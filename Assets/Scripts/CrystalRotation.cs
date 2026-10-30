
using UnityEngine;

public class CrystalRotation : MonoBehaviour
{
    public float rotationSpeed = 60f;

    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate the crystal
        transform.Rotate(
            Vector3.up * rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Float up and down
        float offset = Mathf.Sin(Time.time * floatSpeed)
                       * floatHeight;

        transform.position = startPosition
                             + Vector3.up * offset;
    }
}
