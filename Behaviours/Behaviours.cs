using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HowingMan.Behaviours
{
    public static class Behaviours
    {
        /// <summary>
        /// This is all the calculations that need to happen for the behaviours to work.
        /// This keeps them tidy and easy to work on.
        /// </summary>

        #region Steering Behaviours
        public static Vector2 Seek(Vector2 target, Agent agent)
        {
            return ((target - agent.rigid2D.position).normalized * agent.speed) - agent.rigid2D.velocity;
        }
        public static Vector2 Flee(Vector2 target, Agent agent)
        {
            return ((agent.rigid2D.position - target).normalized * agent.speed) - agent.rigid2D.velocity;
        }
        public static Vector2 Arrive(Vector2 target, Agent agent)
        {
            Vector2 ToTarget = target - agent.rigid2D.position;
            float dist = ToTarget.magnitude;

            if (dist > 0)
            {
                float decelerationTweaker = 0.3f;
                float speed = dist / (3 * decelerationTweaker);

                Vector2 desiredVelocity = ToTarget * speed / dist;
                return desiredVelocity - agent.rigid2D.velocity;
            }

            return Vector2.zero;
        }
        public static Vector2 Pursuit(Transform target, Agent agent) 
        {
            if (target == null)
            {
                Debug.Log("Cannot pursuit, no target set for Agent: " + agent.hashID + ". At location: " + agent.transform.position);
                return Vector2.zero;
            }

            //if ahead and directly facing me, seek.
            Vector2 ToEvader = (Vector2)target.position - agent.rigid2D.position;
            float RelativeHeading = Vector2.Dot(agent.transform.up, target.transform.up); // might need swapping?

            if ((Vector2.Dot(ToEvader, agent.transform.up) > 0) && RelativeHeading < -0.95f)
            {
                return Seek(target.position, agent);
            }

            //else pursuit
            float LookAheadTime = ToEvader.magnitude / (agent.speed + target.GetComponent<Rigidbody2D>().velocity.magnitude);
            return Seek(target.position + target.transform.up * LookAheadTime, agent);
        }
        public static Vector2 Evade(Transform target, Agent agent)
        {
            Vector2 ToPursuer = (Vector2)target.position - agent.rigid2D.position;
            float LookAheadTime = ToPursuer.magnitude / (agent.speed + target.GetComponent<Rigidbody2D>().velocity.magnitude);
            return Flee(target.position + (target.transform.up * LookAheadTime), agent);
        }
        public static Vector2 Wander(Agent agent) // this needs fixing.s
        {
            float WanderRadius = 1f;
            float WanderDistance = 2;

            Vector2 target = agent.transform.position + (agent.transform.up * WanderDistance);
            Vector2 rnd = Random.insideUnitCircle.normalized * WanderRadius;
            target += rnd;

            return Seek(target, agent);

            // This is kinda working. I'm thinking it's not quite as fluid as I want it to be. Can expand on it later when I need it to be something more.
        }
        public static Vector2 ObstacleAvoidance() // not yet 
        {
            return Vector2.zero;
        }
        public static Vector2 WallAvoidance(Agent agent) // this has some problems still is would seem.
        {
            LayerMask mask = LayerMask.GetMask("Background");
            Vector2 steeringForce = Vector2.zero;
           

            RaycastHit2D forwardRay = Physics2D.Raycast(agent.transform.position, agent.transform.up, (1 + agent.rigid2D.velocity.magnitude), mask);
            RaycastHit2D leftRay = Physics2D.Raycast(agent.transform.position, Utility.rotate(agent.transform.up, -45), (1 + agent.rigid2D.velocity.magnitude), mask);
            RaycastHit2D rightRay = Physics2D.Raycast(agent.transform.position, Utility.rotate(agent.transform.up, 45), (1 + agent.rigid2D.velocity.magnitude), mask);
            RaycastHit2D closestHit = new RaycastHit2D();
            //which of these three is the closest points.
            float dist = Mathf.Infinity;


            if (forwardRay.collider != null)
            {
                if ((forwardRay.point - (Vector2)agent.transform.position).sqrMagnitude < dist)
                {
                    closestHit = forwardRay;
                    dist = (forwardRay.point - (Vector2)agent.transform.position).sqrMagnitude;
                }
            }
            if (leftRay.collider != null)
            {
                if ((leftRay.point - (Vector2)agent.transform.position).sqrMagnitude < dist)
                {
                    closestHit = leftRay;
                    dist = (leftRay.point - (Vector2)agent.transform.position).sqrMagnitude;
                }
            }
            if (rightRay.collider != null)
            {
                if ((rightRay.point - (Vector2)agent.transform.position).sqrMagnitude < dist)
                {
                    closestHit = rightRay;
                    dist = (rightRay.point - (Vector2)agent.transform.position).sqrMagnitude;
                }
            }

            if (dist != Mathf.Infinity)
            {
                //calculate by how much we should overshoot. the penetration depth.
                float overShoot = agent.speed - (closestHit.point - (Vector2)agent.transform.position).magnitude;

                //create a steering force based on normal * overshoot;
                steeringForce = (Vector2)agent.transform.up + (closestHit.normal * overShoot);
            }

            return steeringForce;
            // currently working as intended, with a slight problem of getting stuck in corners. 
        }
        public static Vector2 Interpose() // not yet 
        {
            return Vector2.zero;
        }
        public static Vector2 Hide() // not yet 
        {
            return Vector2.zero;
        }
        /*
        public static Vector2 PathFollow(Agent agent) // this doesn't work/
        {
            // move to next target if close enough to current target (working in distance squared space)

            // if our sqr distance to next waypoint is less than our waypoint seek distance sqr. 
             
            if(agent.pathToFollow == null)
            {
                Debug.Log("no path to follow, yet trying to follow one.");
                return Vector2.zero;
            }

            if (agent.currentPathWaypoint + 1 < agent.pathToFollow.Length)
            {
                if ((agent.pathToFollow[agent.currentPathWaypoint + 1] - (Vector2)agent.transform.position).sqrMagnitude < agent.waypointSeekSQR)
                {
                    // set next waypoint. 
                    agent.currentPathWaypoint++;
                }
            }

            // if our next waypoint is not our last point
            // return seek to the next waypoint
            // else return arrive to last waypoint.

            //currently I don't think I need the arrive, so just seek the waypoint.

            if (agent.currentPathWaypoint < agent.pathToFollow.Length)
                return Seek(agent.pathToFollow[agent.currentPathWaypoint], agent); //got a big error here once... not sure why 
            else
            {
                return Vector2.zero;
            }

        }
        */
        public static Vector2 OffsetPursuit() // not yet but need 
        {
            return Vector2.zero;
        }

        #endregion

        #region Advanced Behaviours
 /*
  * old seperate boid simulations, I've pulled  them all into one with weights to reduce the number of calculations each frame
        public static Vector2 Seperation(Agent agent, List<Collider2D> context)
        {
            if (context.Count == 0)
                return Vector2.zero;
               
            Vector2 avoidanceMove = Vector2.zero;
            int nAvoid = 0;

            foreach (Collider2D neighbour in context)
            {
                if (Vector2.SqrMagnitude(neighbour.transform.position - agent.transform.position) < 9) // sqr avidance radius. 3x3?
                {
                    //Vector2 toAgnet = agent.transform.position - neighbour.transform.position;
                    nAvoid++;
                    avoidanceMove += (Vector2)(agent.transform.position - neighbour.transform.position);
                }
            }

            if (nAvoid > 0)
                avoidanceMove /= nAvoid;

            return avoidanceMove;
        }

        public static Vector2 Alignment(Agent agent, List<Collider2D> context)
        {
            if (context.Count == 0)
                return agent.transform.up;

            Vector2 alignmentMove = Vector2.zero;
            int nAvoid = 0;

            foreach (Collider2D neighbour in context)
            {
                if (Vector2.SqrMagnitude(neighbour.transform.position - agent.transform.position) < 9) // sqr avidance radius. 3x3?
                {
                    nAvoid++;
                    alignmentMove += (Vector2)neighbour.transform.up;
                }
            }
            if (nAvoid > 0)
                alignmentMove /= nAvoid;

            return alignmentMove;
        }

        public static Vector2 Cohesion(Agent agent, List<Collider2D> context)
        {
            if (context.Count == 0)
                return Vector2.zero;

            Vector2 cohesionMove = Vector2.zero;
            int nAvoid = 0;

            foreach (Collider2D neighbour in context)
            {
                if (Vector2.SqrMagnitude(neighbour.transform.position - agent.transform.position) < 9) // sqr avidance radius. 3x3?
                {
                    nAvoid++;
                    cohesionMove += (Vector2)neighbour.transform.position;
                }
            }

            if (nAvoid > 0)
                cohesionMove /= nAvoid;

            cohesionMove -= (Vector2)agent.transform.position;
            return cohesionMove;
        }
 */
        public static Vector2 Boids(Agent agent, float seperationAmount, float alignmentAmount, float cohesionAmount)
        {
            // seperation, alignment, cohesion in one.

            // Get neighbours checks for close boids and removes self from list. Does distance check first.
            // this is going to be the bottle neck for sure!
            List<Collider2D> neighbours = GetNeighbours(agent.boidDistanceSQR, agent);

            if (neighbours.Count == 0)
                return agent.transform.up;

            Vector2 boidForce = Vector2.zero;

            foreach (Collider2D neighbour in neighbours)
            {
                boidForce += (Vector2)(agent.transform.position - neighbour.transform.position) * seperationAmount; //seperation
                boidForce += (Vector2)neighbour.transform.up * alignmentAmount; //alignment
                boidForce += (Vector2)neighbour.transform.position * cohesionAmount; // cohesion
            }

            boidForce /= neighbours.Count;

            boidForce -= (Vector2)agent.transform.position;

            return boidForce;
        }
        public static Vector2 StayInRadius (Agent agent, float radius)
        {
            Vector2 center = agent.startLocation;
            Vector2 centerOffset = center - (Vector2)agent.transform.position;
            float t = centerOffset.magnitude / radius;

            if (t < 0.9f)
            {
                return Vector2.zero;
            }

            return centerOffset * t * t;
        }
        static List<Collider2D> GetNeighbours (float distanceCheckSQR, Agent agent)
        {
            List<Collider2D> neighbours = new List<Collider2D>();
            // get a list of only the closest boids to check against. 
            List<Collider2D> CloseNeighbours = AgentManager.instance.GetNeighbours(agent.hashID, agent.agentCollider);

            foreach(Collider2D collider in CloseNeighbours) // use that list instead of the allcolliders here
            {
                //if(collider != agent) // already removed
                //{
                    if((collider.transform.position - agent.transform.position).sqrMagnitude < distanceCheckSQR)
                    {
                        neighbours.Add(collider);
                    }
                //}
            }

            return neighbours;
        }

        #endregion

    }
}