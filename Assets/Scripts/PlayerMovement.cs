using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamState {X, Y,InvX, InvY}


public class PlayerMovement : MonoBehaviour
{
    public float movement_speed = 3.0f;
    public float jump_speed = 6f;
    
    private Vector3 Velocity;
    private Vector3 gravity;
    public Vector3 cameraOffset = new Vector3(0, 0, 0);

    public CamState camera_current_angle = CamState.X;
    public float clone_cooldown = 5.0f;
    public bool is_clone_available = true;

    public GameObject playerPrefab;  // Prefab del jugador
    private GameObject originalPlayer = null;  // Referencia al jugador original
    private GameObject clonePlayer = null;  // Referencia al clon
    public GameObject cam;

    public Rigidbody rb;

    public GameObject spawnPoint;

    public bool dash_available = true;
    public bool is_dashing = false;
    public float dashing_power = 24f;
    public float dashing_time = 0.2f;
    public float dashing_cooldown = 2f;
    [SerializeField] private TrailRenderer trail;

    public float distanceToGround;

    public float rotationSpeed = 5f;
    private bool controllingClone = false;

    public Vector3 totalVel;

    public int current_dimension = 0; 

    private bool double_jump_allowed = false;

    private bool isRotating = false;

    void Start()
    {
        originalPlayer = this.gameObject;
        rb = GetComponent<Rigidbody>();
        distanceToGround = GetComponent<Collider>().bounds.extents.y;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        Velocity = new Vector3(0,0,0);
        originalPlayer.transform.position = spawnPoint.transform.position;
    }

    void Update()
    {
        if (originalPlayer.transform.position.y <= 0)
        {
            Respawn();
        }
        Movement(originalPlayer);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerJump(originalPlayer);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            RotatePlayer(originalPlayer, -90f);
            if (clonePlayer != null)
                RotatePlayer(clonePlayer, -90f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            RotatePlayer(originalPlayer, 90f);
            if (clonePlayer != null)
                RotatePlayer(clonePlayer, 90f);
        }
        
        if (Input.GetKeyDown(KeyCode.C) && !controllingClone  && clonePlayer == null && is_clone_available){
            CreateClone();
        }
        else if (Input.GetKeyDown(KeyCode.V) && controllingClone )  
        {
            StartCoroutine(SwitchToOriginalPlayer());
        }
        if (is_dashing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dash_available)
        {
            StartCoroutine(Dash());
        }
    }

    void RotatePlayer(GameObject player, float angle)
    {
        if (!isRotating)
        {
            StartCoroutine(RotateSmoothly(player, angle));
        }
        
        // Actualiza la dimensión actual del jugador
        float viewAngle = (angle / 90) != 0 ? 1f : -1f;
        current_dimension = (int)(current_dimension + viewAngle + 4) % 4;
    }

    IEnumerator RotateSmoothly(GameObject player, float angle)
    {
        isRotating = true;

        // Obtén la rotación inicial del jugador
        Quaternion initialRotation = player.transform.rotation;
        
        // Calcula la rotación objetivo
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, angle, 0);

        float time = 0f;

        while (time < 1f)
        {
            // Interpola la rotación usando Lerp
            player.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;

            // Espera hasta el siguiente frame
            yield return null;
        }

        // Asegúrate de que la rotación final sea exacta
        player.transform.rotation = targetRotation;

        isRotating = false;
    }

    private void Respawn()
    {
        originalPlayer.transform.position = spawnPoint.transform.position;
    }

    private IEnumerator SwitchToOriginalPlayer()
    {
        controllingClone = false;  // Ahora volvemos a controlar el jugador original
        originalPlayer.transform.position = clonePlayer.transform.position;
        Destroy(clonePlayer);
        clonePlayer = null;

        is_clone_available = false;

        yield return new WaitForSeconds(clone_cooldown);

        is_clone_available = true;
    }

    private void DisableCollider(GameObject clone)
    {
        Collider cloneCollider = clone.GetComponent<Collider>();
        if (cloneCollider != null)
        {
            // Opción 1: Desactivar el Collider por completo
            cloneCollider.enabled = false;

            // Opción 2: Cambiar el Collider a un Trigger (para que sea detectable pero no tangible)
            // cloneCollider.isTrigger = true;
        }
    }

    private void CreateClone()
    {
        clonePlayer = Instantiate(playerPrefab, originalPlayer.transform.position, originalPlayer.transform.rotation);
        controllingClone = true;
    }

    private void Movement(GameObject player)
    {
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.A) && !isInFront(player, player.transform.right))
        {
            // Mover hacia la izquierda según la orientación de la cámara
            movement -= cam.transform.right;
        }
        if (Input.GetKey(KeyCode.D) && !isInFront(player, player.transform.right * -1))
        {
            // Mover hacia la derecha según la orientación de la cámara
            movement += cam.transform.right;
        }

        // Normaliza el vector de movimiento para que tenga una magnitud uniforme y multiplícalo por la velocidad
        movement = movement.normalized * movement_speed;

        // Mantén la velocidad vertical actual y aplica la nueva velocidad en X y Z
        Rigidbody body = player.GetComponent<Rigidbody>();

        body.velocity = new Vector3(movement.x, body.velocity.y, movement.z);
    }
    
    private IEnumerator Dash()
    {
        dash_available = false;
        is_dashing = true;

        float originalDrag = rb.drag;
        rb.useGravity = false; 
        rb.drag = 0f;
        
        Vector3 tpPosition = transform.position;

        Vector3 movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) 
        {
            movementDirection -= transform.forward; 
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementDirection += transform.forward; 
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementDirection += transform.right; 
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementDirection -= transform.right; 
        }

        movementDirection = movementDirection.normalized;

        if (movementDirection == Vector3.zero)
        {
            if (current_dimension == 1)
            {
                movementDirection = -transform.forward; 
            }
            else
            {
                movementDirection = -transform.right; 
            }
        }

        tpPosition += movementDirection * dashing_power;

        tpPosition.y = transform.position.y;

        rb.position = tpPosition;

        yield return new WaitForSeconds(dashing_time);

        rb.useGravity = true;
        rb.drag = originalDrag;
        is_dashing = false;

        yield return new WaitForSeconds(dashing_cooldown);
        dash_available = true;
    }


    private void PlayerJump(GameObject player) 
    {
        Rigidbody addVel = player.GetComponent<Rigidbody>();

        if (IsGrounded(player) || double_jump_allowed)
        {
            if (IsGrounded(player))
            {
                double_jump_allowed = true;
            }
            else
            {
                double_jump_allowed = false;
            }
            addVel.velocity = new Vector3(addVel.velocity.x, jump_speed, addVel.velocity.z);
        }
    }

    private bool isInFront(GameObject player, Vector3 direction)
    {
        // Centro del OverlapBox, un poco por debajo del jugador para detectar el suelo.
        Vector3 boxCenter = player.transform.position + direction * 0.1f;
        
        // Tamaño del OverlapBox. Los half extents son la mitad de las dimensiones de la caja.
        Vector3 boxHalfExtents = new Vector3(0.1f, 0.01f, 0.1f); // Ajusta los valores para que se adapten al tamaño del jugador.
        
        // Capa del suelo. Usa "Ground" si tienes una capa específica, o usa el valor por defecto si no.
        int groundLayerMask = LayerMask.GetMask("Floor");

        // Comprueba si hay algún collider del suelo en el área del OverlapBox.
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, groundLayerMask, QueryTriggerInteraction.Ignore);

        // Si encuentra algún collider, significa que el jugador está tocando el suelo.
        return colliders.Length > 0;
    }

    private bool IsGrounded(GameObject player)
    {
        // Centro del OverlapBox, un poco por debajo del jugador para detectar el suelo.
        Vector3 boxCenter = player.transform.position + Vector3.down * 0.1f;
        
        // Tamaño del OverlapBox. Los half extents son la mitad de las dimensiones de la caja.
        Vector3 boxHalfExtents = new Vector3(0.1f, 0.01f, 0.1f); // Ajusta los valores para que se adapten al tamaño del jugador.
        
        // Capa del suelo. Usa "Ground" si tienes una capa específica, o usa el valor por defecto si no.
        int groundLayerMask = LayerMask.GetMask("Floor");

        // Comprueba si hay algún collider del suelo en el área del OverlapBox.
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, groundLayerMask, QueryTriggerInteraction.Ignore);
        // Debug.Log(colliders.Length > 0);
        // Si encuentra algún collider, significa que el jugador está tocando el suelo.
        return colliders.Length > 0;
    }

    void OnDrawGizmos()
    {
        // Solo dibujar los Gizmos si el objeto está activo.
        if (originalPlayer != null)
        {
            DrawGroundCheckGizmos(originalPlayer);
        }

        if (controllingClone && clonePlayer != null)
        {
            DrawGroundCheckGizmos(clonePlayer);
        }
    }

    // Método para dibujar el Gizmo del OverlapBox para la detección de suelo.
    private void DrawGroundCheckGizmos(GameObject player)
    {
        // Centro del OverlapBox, un poco por debajo del jugador para detectar el suelo.
        Vector3 boxCenter = player.transform.position + Vector3.down * 0.1f;

        // Tamaño del OverlapBox. Los half extents son la mitad de las dimensiones de la caja.
        Vector3 boxHalfExtents = new Vector3(0.1f, 0.01f, 0.1f); // Ajusta los valores para que se adapten al tamaño del jugador.

        // Configurar el color del Gizmo.
        Gizmos.color = Color.red;

        // Dibujar el cubo del OverlapBox en la posición y tamaño especificados.
        Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
    }


}
