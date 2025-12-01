using UnityEngine;
using System;

public class NPCWaypointMovement : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints; // WP_0..WP_5

    [Header("Movement")]
    public float speed = 1f;
    public float rotationSpeed = 5f;

    [Header("State")]
    public bool isWaitingDecision = false;

    private int currentIndex = 0;
    private bool isMoving = false;

    public Action onReachedWaypoint1;
    public Action onReachedLastWaypoint;

    void Update()
    {
        if (!isMoving || waypoints == null || waypoints.Length == 0 || currentIndex >= waypoints.Length)
            return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = (target.position - transform.position);
        Vector3 flatDir = new Vector3(direction.x, 0f, direction.z).normalized;

        // Movimiento (usa dirección 3D para posicion; esto evita problemas si target tiene distinta Y)
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Rotación suave solo en Y (más natural)
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Llegada
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            // Si llegó a WP_1 -> detener y esperar decisión
            if (currentIndex == 1)
            {
                isMoving = false;
                isWaitingDecision = true;
                onReachedWaypoint1?.Invoke();
            }
            // Si llegó al último waypoint -> notificar y destruir
            else if (currentIndex == waypoints.Length - 1)
            {
                onReachedLastWaypoint?.Invoke();
                Destroy(gameObject);
            }
            else
            {
                // Avanzar al siguiente
                currentIndex++;
            }
        }
    }

    public void StartWalking()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        currentIndex = 0;
        isWaitingDecision = false;
        isMoving = true;
    }

    // Llamar cuando el jugador acepta (cielo)
    public void ContinueWalkingToHeaven()
    {
        if (!isWaitingDecision) return;
        isWaitingDecision = false;
        // Si aún está en WP_1 asegurarse de saltar a WP_2 (si existe)
        if (currentIndex == 1 && waypoints.Length > 2)
            currentIndex = 2;
        isMoving = true;
    }
}
