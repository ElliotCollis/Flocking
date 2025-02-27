using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private bool followActive = true;
    public bool followIsActive
    {
        get
        {
            return followActive;
        }
        set
        {
            if (value) rigid2D.velocity = Vector2.zero;
            followActive = value;
        }
    }

    public Transform target = null;
    public float speed = 1f;
    public float followDistance = 10f;
    public bool randomWalkWhenNotFollowing = false;
    public float walkDistance = 2f;
    public float patrolStopTimer = 1f;
    public bool returnHomeWhenOutOfRange = true;

    [Space]
    public bool flipY = false;
    public bool flipX = false;
    public bool rotateToFollow = false;
    public bool invertY = false;
    public bool invertX = false;
    public Transform flipSprite = null;

    bool inRange = false;
    Vector2 startPosition;
    float followDistSqr;
    [HideInInspector] public float distancetoTargetSqr;
    float patrolTimer = 0;
   [SerializeField] Rigidbody2D rigid2D;
    List<Vector2> walkTargets;
    Vector2 currentWalkTarget;

    async void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        followDistSqr = followDistance * followDistance;
        if (target != null) SetTarget(target); 

        if(randomWalkWhenNotFollowing)
        {
            walkTargets = new List<Vector2>();
            for (int i = 0; i < 5; i++)
            {
                walkTargets.Add((Vector2)transform.position + Random.insideUnitCircle * walkDistance);
            }
            currentWalkTarget = walkTargets[Random.Range(0, walkTargets.Count)];
            walkTargets.Remove(currentWalkTarget);
        }
    }

    public void SetTarget (Transform t)
    {
        target = t;
        distancetoTargetSqr = (transform.position - target.position).sqrMagnitude;
    }

    void FixedUpdate()
    {
        if (followActive)
        {
            if (target != false)
            {
                distancetoTargetSqr = (transform.position - target.position).sqrMagnitude;

                inRange = distancetoTargetSqr < followDistSqr;

                if (inRange)
                {
                    rigid2D.MovePosition(Vector2.MoveTowards(transform.position, target.position, Time.fixedDeltaTime * speed));
                }
                else if (randomWalkWhenNotFollowing)
                {
                    if (Time.time > patrolTimer)
                    {

                        if (IsApproximatelySame(transform.position, currentWalkTarget, 0.2f)) // change currentWalkTarget
                        {
                            Vector2 tempVector = currentWalkTarget;
                            currentWalkTarget = walkTargets[Random.Range(0, walkTargets.Count)];
                            walkTargets.Remove(currentWalkTarget);
                            walkTargets.Add(tempVector);

                            patrolTimer = Time.time + patrolStopTimer;
                        }
                        rigid2D.MovePosition(Vector2.MoveTowards(transform.position, currentWalkTarget, Time.fixedDeltaTime * speed));
                    }
                }
                else
                    rigid2D.MovePosition(Vector2.MoveTowards(transform.position, startPosition, Time.fixedDeltaTime * speed));

                //aesthetics

                // here it needs to be a relavant scale, not just 1?
                if(flipY)
                {
                    if(target.transform.position.y > transform.position.y) // currently setting every frame
                    {
                        Vector3 scale = flipSprite != null ? flipSprite.localScale : transform.localScale;
                        scale.y = invertY ? -1 : 1; // assumes that we're using a 1,1,1 scale on this object.
                        if (flipSprite != null)
                            flipSprite.localScale = scale;
                        else
                            transform.localScale = scale;
                    }else
                    {
                        Vector3 scale = flipSprite != null ? flipSprite.localScale : transform.localScale;
                        scale.y = invertY ? 1 : -1; // assumes that we're using a 1,1,1 scale on this object.
                        if (flipSprite != null)
                            flipSprite.localScale = scale;
                        else
                            transform.localScale = scale;
                    }
                }

                if (flipX)
                {
                    if (target.transform.position.x > transform.position.x)
                    {
                        Vector3 scale = flipSprite != null ? flipSprite.localScale : transform.localScale;
                        scale.x = invertX ? -1 : 1;
                        if (flipSprite != null)
                            flipSprite.localScale = scale;
                        else
                            transform.localScale = scale;
                    }
                    else
                    {
                        Vector3 scale = flipSprite != null ? flipSprite.localScale : transform.localScale;
                        scale.x = invertX ? 1 : -1;
                        if (flipSprite != null)
                            flipSprite.localScale = scale;
                        else
                            transform.localScale = scale;
                    }
                }

                if(rotateToFollow) // gonne use the red arrow for now, transmform.right.
                {
                    if (flipX)
                    {
                        if (target.transform.position.x > transform.position.x)
                        {
                            // rotate right and left, needs an angle limiter maybe.
                            // but can't think right now so I'll just not do this yet..
                        }
                        else
                        {
                        }
                            
                    }
                    else
                    {
                        Vector2 targetRot = (target.transform.position - transform.position).normalized;
                        transform.right = Vector2.Lerp(transform.right, targetRot, Time.deltaTime);
                    }
                }
            }
        }
    }

    public bool IsApproximatelySame(Vector2 vector1, Vector2 vector2, float threshold)
    {
        float deltaX = vector1.x - vector2.x;
        float deltaY = vector1.y - vector2.y;
        return (deltaX * deltaX + deltaY * deltaY) <= threshold * threshold;
    }

}
