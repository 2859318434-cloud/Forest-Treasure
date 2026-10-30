
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 360f;
    public float jumpForce = 7f;
    public Transform cameraTransform;
    public Animator animator;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 input;
    private bool isGrounded;
    private bool canJump = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            input = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        input = new Vector2(horizontal, vertical);
        animator.SetFloat("Speed", input.magnitude);

        // 检测角色脚下是否有地面
        Vector3 origin = capsule.bounds.center
            + Vector3.down * (capsule.bounds.extents.y
            - capsule.radius * 0.9f);

        isGrounded = Physics.SphereCast(
            origin,
            capsule.radius * 0.9f,
            Vector3.down,
            out RaycastHit hit,
            0.2f
        );

        // 落地后重新允许跳跃
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            canJump = true;
        }

        // 空格键跳跃：暂时不触发动画
        if (Input.GetKeyDown(KeyCode.Space)
            && isGrounded
            && canJump)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
            animator.SetTrigger("Jump");
            canJump = false;
        }
    }

    void FixedUpdate()
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movement =
            cameraForward * input.y +
            cameraRight * input.x;

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(movement);

            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);
        }

        rb.MovePosition(
            rb.position +
            movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}

