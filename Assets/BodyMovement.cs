using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    public float speed = 10f;
    public float verticalSpeed = 1f;
    public float rotationSpeed = 1f;
    public float deviation = 1f;

    public float maxHeight = 1.5f;
    public float middleHeight = 1f;

    public LimbMovement[] rightLimbs;
    public LimbMovement[] leftLimbs;

    public Transform centerPoint;

    public float gismosRadius = 0.1f;

    [HideInInspector]
    public float direction = 1f;

    private Rigidbody2D mainRigidbody;

    private bool isInfiinityMove = false;


    private Vector3 GetMiddleRightTouchPoint()
    {
        Vector3 sum = Vector3.zero;
        foreach (LimbMovement limbMovement in rightLimbs)
        {
            sum += limbMovement.GetNearTouchPoint();
        }

        return sum / rightLimbs.Length;
    }

    private Vector3 GetMiddleLeftTouchPoint()
    {
        Vector3 sum = Vector3.zero;
        foreach (LimbMovement limbMovement in leftLimbs)
        {
            sum += limbMovement.GetNearTouchPoint();
        }

        return sum / leftLimbs.Length;
    }

    private float GetAngle()
    {
        Vector3 middleRightTouchPoint = GetMiddleRightTouchPoint();
        Vector3 middleLeftTouchPoint = GetMiddleLeftTouchPoint();

        Vector3 direction = middleRightTouchPoint - middleLeftTouchPoint;

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private Vector3 GetCenterPoint()
    {
        return centerPoint.position;
    }

    private Vector3 GetBottomCenterPoint()
    {
        return centerPoint.position - centerPoint.up * maxHeight;
    }
    
    private Vector3 GetCenterTouchPoint()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(GetCenterPoint(), -centerPoint.up, maxHeight, LayerMask.GetMask("environment"));

        if (hitInfo)
        {
            return hitInfo.point;
        }
        else
        {
            return GetBottomCenterPoint();
        }
    }

    private float GetHeight()
    {
        return Vector3.Distance(GetCenterPoint(), GetCenterTouchPoint());
    }

    private void Start()
    {
        mainRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.X))
        {
            isInfiinityMove = !isInfiinityMove;
            direction = 1f;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            isInfiinityMove = !isInfiinityMove;
            direction = -1f;
        }

        if (isInfiinityMove)
        {
            horizontal = direction;
        }
        else
        {
            direction = Input.GetAxisRaw("Horizontal");
        }

        float currentSpeed = horizontal * speed;

        Vector3 nextPosition = transform.position + transform.right * currentSpeed;

        float height = middleHeight + deviation * Mathf.Sin(Time.time * verticalSpeed);
        nextPosition = Vector3.Lerp(nextPosition, nextPosition - transform.up * (GetHeight() - height), Time.deltaTime * verticalSpeed);

        mainRigidbody.MovePosition(nextPosition);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, GetAngle()), Time.deltaTime * rotationSpeed);
    }


    private void OnDrawGizmos()
    {
        Vector3 centerTouchPoint = GetCenterTouchPoint();

        Gizmos.color = Color.green;
        Gizmos.DrawLine(GetCenterPoint(), GetBottomCenterPoint());
        Gizmos.DrawSphere(centerTouchPoint, gismosRadius);
    }
}
