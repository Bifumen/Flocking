using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour {

    [Header("Properties")]
    public Transform group = null;//Transform of an object whose children we are going to loop through
    public float range = 2.5f;//How far away does our GameObject have to be from other members of the group to apply flocking
    public float flockingInterval = 0.6f;//How often we are going to refresh Alignment,Cohesion and Separation

    Vector2 Alignment;
    Vector2 Cohesion;
    Vector2 Separation;

    [Header("Weights")]
    public float AlignmentWeight = 1f;
    public float CohesionWeight = 1f;
    public float SeparationWeight = 1f;

    [Space]
    public bool drawGizmos = true;

    // Use this for initialization
    void Start () {
        StartCoroutine(FlockingPeriod());
	}

    void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, range);
        }
    }

    //Returns Alignment,Cohesion and Separation added up into one vector
    public Vector2 GetFlockingVector()
    {
        return Alignment * AlignmentWeight + Cohesion * CohesionWeight + Separation * SeparationWeight;
    }

    //Adds the flocking vector to the GameObject's velocity, normalizes it and multiples it with the speed constant 
    public Vector2 GetNormalizedFlockingVector(Vector2 velocity,float speed)
    {
        return (velocity + GetFlockingVector()).normalized * speed;
    }

    Vector2 GetAlignment()
    {
        Vector2 v = Vector2.zero;//Default value
        int neighborCount = 0;
        GameObject agent;

        for (int i = 0; i < group.childCount; i++)
        {
            agent = group.GetChild(i).gameObject;

            if(agent != gameObject && agent.activeInHierarchy)
            {
                float dist = (agent.transform.position - transform.position).sqrMagnitude;

                if(dist< range * range)//If other objects in the group are in range we add their velocity to our vector
                {
                    Vector2 agentVelocity = agent.GetComponent<CharacterController2D>().velocity;

                    v += agentVelocity;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
            v /= neighborCount;//We divide our vector with the number of GameObjects in range

        return v.normalized;
    }

    Vector2 GetCohesion()
    {
        Vector2 v = Vector2.zero;//Default value
        int neighborCount = 0;
        GameObject agent;

        for (int i = 0; i < group.childCount; i++)
        {
            agent = group.GetChild(i).gameObject;

            if (agent != gameObject && agent.activeInHierarchy)
            {
                float dist = (agent.transform.position - transform.position).sqrMagnitude;

                if (dist < range * range)//If other objects in the group are in range we add their position to the vector
                {
                    v += (Vector2)agent.transform.position;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            v /= neighborCount;//We divide our vector with the number of GameObjects in range to find the center position

            v = v - (Vector2)transform.position;//We calculate the distance from our GameObject to the center
        }

        return v.normalized;
    }

    Vector2 GetSeparation()
    {
        Vector2 v = Vector2.zero;//Default value
        int neighborCount = 0;
        GameObject agent;

        for (int i = 0; i < group.childCount; i++)
        {
            agent = group.GetChild(i).gameObject;

            if (agent != gameObject && agent.activeInHierarchy)
            {
                float dist = (agent.transform.position - transform.position).sqrMagnitude;

                if (dist < range * range)//If other objects in the group are in range we add the distance to the vector
                {
                    v += (Vector2)agent.transform.position - (Vector2)transform.position;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            v /= neighborCount;//We divide our vector with the number of GameObjects in range and inverse it

            v = v * -1;
        }

        return v.normalized;
    }

    IEnumerator FlockingPeriod()//The coroutine which periodically refreshes our values
    {
        Alignment = GetAlignment();
        Cohesion = GetCohesion();
        Separation = GetSeparation();

        yield return new WaitForSeconds(flockingInterval);
        StartCoroutine(FlockingPeriod());
    }

}
