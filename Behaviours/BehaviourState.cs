using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HowingMan.Behaviours;

public abstract class BehaviourState
{
    public abstract Vector2 CalculateForce(Agent me, Transform target);
}

public class IdleBehaviour : BehaviourState //rename idle
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;
        //List<Collider2D> colliders = Utility.GetColliders(agents, me);

       // force += Behaviours.Wander(me); // need to fix wander, it's too random atm.
        force += Behaviours.Boids(me, 1.5f, 1, 1);
        force += Behaviours.StayInRadius(me, 2);
        force += Behaviours.WallAvoidance(me); // this might need uping. 

        force = Vector2.ClampMagnitude(force, me.MaxSpeed);
        return force;
    }
}
public class HuntGroupBehaviour : BehaviourState
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;

        force += Behaviours.Pursuit(target, me);
        force += Behaviours.Boids(me, 2, 1, 1);
        force += Behaviours.WallAvoidance(me) ;

        return force;
    }
}
public class HuntBehaviour : BehaviourState
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;
        force += Behaviours.Pursuit(target, me);
        force += Behaviours.WallAvoidance(me);
        return force;
    }
}
public class SearchForPlayer : BehaviourState
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;
        force += Behaviours.Seek(me.lastPlayerLocation, me);
        force += Behaviours.WallAvoidance(me);
        return force;
    }
}
public class SeekHome : BehaviourState
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;
        force += Behaviours.Seek(me.startLocation, me);
        force += Behaviours.WallAvoidance(me);
        return force;
    }
}
public class SeekStrongBehaviour : BehaviourState
{
    public override Vector2 CalculateForce(Agent me, Transform target)
    {
        Vector2 force = Vector2.zero;
        force += Behaviours.Seek(target.position, me) * 3;
        return force;
    }
}
