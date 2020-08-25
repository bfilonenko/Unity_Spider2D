using UnityEngine;

public class LimbMovement : MonoBehaviour
{
    public BodyMovement bodyMovement;

    public Transform limbEffector;
    public Transform limbCCD;

    public LimbMovement followByLimbMovement;
    public Transform followBy;

    public bool isRightLimb = true;

    public float moveDuration = 0.5f;

    public float maxHeight = 2.5f;
    public float distanceBetweenNearAndFarPoints = 3f;

    public float distanceToStartMoving = 1.5f;

    public float coefficientOfParabolaHeight = 1;

    public float gismosRadius = 0.1f;

    public bool needMove = false;

    private Vector3 startPosition;
    private Vector3 endPostion;

    // Movement parameters:
    // y = - cx^2 + ax + b
    // c - coefficientOfParabolaHeight
    private float movementParameterA;
    private float movementParameterB;

    private float elapsed = 0f;


    public Vector3 GetNearTouchPoint()
    {
        return GetTouchPoint(GetNearPoint());
    }

    public Vector3 GetFarTouchPoint()
    {
        return GetTouchPoint(GetFarPoint());
    }


    private Vector3 GetNearPoint()
    {
        if (isRightLimb)
        {
            return transform.position - transform.right * distanceBetweenNearAndFarPoints;
        }
        else
        {
            return transform.position + transform.right * distanceBetweenNearAndFarPoints;
        }
    }

    private Vector3 GetBottomNearPoint()
    {
        return GetNearPoint() - transform.up * maxHeight;
    }

    private Vector3 GetFarPoint()
    {
        return transform.position;
    }

    private Vector3 GetBottomFarPoint()
    {
        return GetFarPoint() - transform.up * maxHeight;
    }

    private Vector3 GetTouchPoint(Vector3 point)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(point, -transform.up, maxHeight, LayerMask.GetMask("environment"));

        if (hitInfo)
        {
            return hitInfo.point;
        }
        else
        {
            return point - transform.up * maxHeight;
        }
    }

    private void CalculateMovementParameters()
    {
        float y1 = endPostion.y;
        float x1 = endPostion.x;
        float y2 = startPosition.y;
        float x2 = startPosition.x;

        movementParameterA = (y1 - y2 + coefficientOfParabolaHeight * (x1 * x1 - x2 * x2)) / (x1 - x2);
        movementParameterB = y1 + coefficientOfParabolaHeight * x1 * x1 - movementParameterA * x1;
    }

    private void ResetMovement(Vector3 endPoint)
    {
        startPosition = limbCCD.position;
        endPostion = endPoint;
        elapsed = 0f;
        needMove = true;
        CalculateMovementParameters();
    }

    private bool IsNextNearPoint(float direction)
    {
        return !needMove && ((direction < 0f && isRightLimb) || (direction > 0f && !isRightLimb));
    }

    private bool IsNextFarPoint(float direction)
    {
        return !needMove && ((direction > 0f && isRightLimb) || (direction < 0f && !isRightLimb));
    }

    private void FixedUpdate()
    {
        Vector3 farTouchPoint = GetFarTouchPoint();
        Vector3 nearTouchPoint = GetNearTouchPoint();

        float direction = bodyMovement.direction;

        bool canMove = true;
        if (followBy)
        {
            canMove = false;

            if (!followByLimbMovement.needMove)
            {
                Vector3 position = followBy.position;
                if (IsNextNearPoint(direction) && (position - nearTouchPoint).sqrMagnitude > (distanceToStartMoving * distanceToStartMoving) / 4f)
                {
                    canMove = true;
                }
                else if (IsNextFarPoint(direction) && (position - farTouchPoint).sqrMagnitude > (distanceToStartMoving * distanceToStartMoving) / 4f)
                {
                    canMove = true;
                }
            }
        }

        if (canMove)
        {
            if (IsNextNearPoint(direction) &&
                (nearTouchPoint - limbCCD.position).sqrMagnitude > distanceToStartMoving * distanceToStartMoving)
            {
                ResetMovement(nearTouchPoint);
            }
            else if (IsNextFarPoint(direction) &&
                (farTouchPoint - limbCCD.position).sqrMagnitude > distanceToStartMoving * distanceToStartMoving)
            {
                ResetMovement(farTouchPoint);
            }
        }

        if (elapsed > moveDuration)
        {
            needMove = false;
        }

        if (needMove)
        {
            elapsed += Time.deltaTime;

            float x = Mathf.Lerp(startPosition.x, endPostion.x, elapsed / moveDuration);
            float y = -coefficientOfParabolaHeight * x * x + movementParameterA * x + movementParameterB;

            limbCCD.position = new Vector3(x, y, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 farTouchPoint = GetFarTouchPoint();
        Vector3 nearTouchPoint = GetNearTouchPoint();

        Gizmos.color = Color.white;
        Gizmos.DrawLine(farTouchPoint, limbCCD.position);
        Gizmos.DrawLine(nearTouchPoint, limbCCD.position);

        Gizmos.DrawWireSphere(farTouchPoint, distanceToStartMoving);
        Gizmos.DrawWireSphere(nearTouchPoint, distanceToStartMoving);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(GetFarPoint(), GetBottomFarPoint());
        Gizmos.DrawSphere(farTouchPoint, gismosRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(GetNearPoint(), GetBottomNearPoint());
        Gizmos.DrawSphere(nearTouchPoint, gismosRadius);
    }
}
