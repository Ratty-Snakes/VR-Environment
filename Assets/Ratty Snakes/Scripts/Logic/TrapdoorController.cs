using UnityEngine;

public class TrapdoorController : MonoBehaviour
{
    [Header("Referencias a los Pivotes")]
    public Transform leftPivot;  // Arrastra aquí Trapdoor_LeftPivot
    public Transform rightPivot; // Arrastra aquí Trapdoor_RightPivot

    [Header("Configuración de Rotación")]
    // Ajusta estos valores en el inspector probando manualmente qué rotación abre la puerta hacia abajo
    public Vector3 leftOpenRotation = new Vector3(0, 0, -90);
    public Vector3 rightOpenRotation = new Vector3(0, 0, 90);

    public float animationSpeed = 5f;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private bool isOpen = false;

    void Start()
    {
        // Guardamos la rotación inicial como "Cerrada"
        if (leftPivot) leftClosedRot = leftPivot.localRotation;
        if (rightPivot) rightClosedRot = rightPivot.localRotation;

        // Calculamos la rotación "Abierta"
        leftOpenRot = Quaternion.Euler(leftOpenRotation);
        rightOpenRot = Quaternion.Euler(rightOpenRotation);
    }

    void Update()
    {
        if (leftPivot == null || rightPivot == null) return;

        if (isOpen)
        {
            // Abrir suavemente
            leftPivot.localRotation = Quaternion.Slerp(leftPivot.localRotation, leftOpenRot, Time.deltaTime * animationSpeed);
            rightPivot.localRotation = Quaternion.Slerp(rightPivot.localRotation, rightOpenRot, Time.deltaTime * animationSpeed);
        }
        else
        {
            // Cerrar suavemente
            leftPivot.localRotation = Quaternion.Slerp(leftPivot.localRotation, leftClosedRot, Time.deltaTime * animationSpeed);
            rightPivot.localRotation = Quaternion.Slerp(rightPivot.localRotation, rightClosedRot, Time.deltaTime * animationSpeed);
        }
    }

    // Estos métodos los llamará el NPCManager o la Palanca
    public void OpenTrapdoor()
    {
        isOpen = true;
        Debug.Log(">> TRAMPILLA: Abriendo compuertas.");
    }

    public void CloseTrapdoor()
    {
        isOpen = false;
        Debug.Log(">> TRAMPILLA: Cerrando compuertas.");
    }
}