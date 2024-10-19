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

    public GameObject playerPrefab;  // Prefab del jugador
    private GameObject originalPlayer = null;  // Referencia al jugador original
    private GameObject clonePlayer = null;  // Referencia al clon
    public GameObject cam;

    public Rigidbody rb;

    bool dash_available = true;
    bool is_dashing = false;
    float dashing_power = 24f;
    float dashing_time = 0.2f;
    float dashing_cooldown = 2f;
    [SerializeField] private TrailRenderer trail;

    public float distanceToGround;

    private bool controllingClone = false;

    public Vector3 totalVel;

    void Start()
    {
        originalPlayer = this.gameObject;
        rb = GetComponent<Rigidbody>();
        distanceToGround = GetComponent<Collider>().bounds.extents.y;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        Velocity = new Vector3(0,0,0);
    }

    void Update()
    {
        Movement(originalPlayer);
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded(originalPlayer))
        {
            PlayerJump(originalPlayer);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotatePlayer(originalPlayer, 90f);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotatePlayer(originalPlayer, -90f);
        }
        
        if (Input.GetKeyDown(KeyCode.C) && !controllingClone  && clonePlayer == null){
            CreateClone();
        }
        else if (Input.GetKeyDown(KeyCode.V) && controllingClone)  
        {
            SwitchToOriginalPlayer();
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
        player.transform.Rotate(0, angle, 0);
    }

    void SwitchToOriginalPlayer()
    {
        controllingClone = false;  // Ahora volvemos a controlar el jugador original
        originalPlayer.transform.position = clonePlayer.transform.position;
        Destroy(clonePlayer);
        clonePlayer = null;
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

        if (Input.GetKey(KeyCode.A))
        {
            // Mover hacia la izquierda según la orientación de la cámara
            movement -= cam.transform.right;
        }
        if (Input.GetKey(KeyCode.D))
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
        Debug.Log("Dash");
        dash_available = false;
        is_dashing = true; 
        rb.useGravity = false;
        rb.velocity = new Vector3(transform.localScale.x * dashing_power, 0f, 0f);
        trail.emitting = true;
        yield return new WaitForSeconds(dashing_time);
        trail.emitting = false;
        rb.useGravity  = true;
        is_dashing = false;
        yield return new WaitForSeconds(dashing_cooldown);
        dash_available = true;
    }

    private void PlayerJump(GameObject player) 
    {
        if (!IsGrounded(player)) return;
        Rigidbody addVel =  player.GetComponent<Rigidbody>();
        addVel.velocity = new Vector3(addVel.velocity.x, jump_speed, addVel.velocity.z);
        //totalVel += new Vector3(0, jump_speed, 0);

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
        Debug.Log(colliders.Length > 0);
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
        Vector3 boxHalfExtents = new Vector3(0.1f, 0.1f, 0.1f); // Ajusta los valores para que se adapten al tamaño del jugador.

        // Configurar el color del Gizmo.
        Gizmos.color = Color.red;

        // Dibujar el cubo del OverlapBox en la posición y tamaño especificados.
        Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
    }


}
