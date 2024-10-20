using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    public Transform startPoint;  // Punto de inicio
    public Transform endPoint;    // Punto de destino
    public float moveTime = 2.0f; // Tiempo que tardará en moverse

    private float elapsedTime = 0f;
    private bool isMovingToEnd = true;  // Controla si está yendo al destino o al inicio
    private bool isMoving = false;

    void Update()
    {
        // Inicia el movimiento al presionar la tecla "M"
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartMoving();
        }

        // Si está en movimiento, realiza el desplazamiento
        if (isMoving)
        {
            MoveOverTime();
        }
    }

    void StartMoving()
    {
        elapsedTime = 0f;  // Reinicia el tiempo transcurrido
        isMoving = true;   // Comienza el movimiento
    }

    void MoveOverTime()
    {
        elapsedTime += Time.deltaTime; // Incrementa el tiempo transcurrido

        // Calcula el porcentaje del tiempo de movimiento completado
        float progress = elapsedTime / moveTime;

        // Mueve el objeto según la dirección actual (al punto final o de regreso al inicial)
        if (isMovingToEnd)
        {
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);
        }
        else
        {
            transform.position = Vector3.Lerp(endPoint.position, startPoint.position, progress);
        }

        // Cuando llega al final, se devuelve al inicio
        if (progress >= 1.0f)
        {
            // Alterna entre moverse al final o al inicio
            isMovingToEnd = !isMovingToEnd;
            elapsedTime = 0f; // Reinicia el tiempo para la vuelta
        }
    }
}
