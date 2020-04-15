using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlienController : MonoBehaviour
{

    public NavMeshAgent agent;
    public Animator animator;
    public bool panic;

    Vector3 destination;
    int delay = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.isStopped = true;
        panic = false;
        destination = transform.position;
    }

    private void FixedUpdate() {
        if(panic){
            if(agent.isStopped){
                agent.isStopped = false;
                animator.SetTrigger("armAction");
            } else {
                if(Vector3.Distance(destination, transform.position) < 25){
                    destination = panicMode();
                    agent.SetDestination(destination);
                }
            }

            delay++;
            if(delay >= 240){
                animator.SetTrigger("armAction"); // uses Unity's Animation controller
                delay = 0;
            }
        }    
    }

    // agent will randomly move to various navigation points
    Vector3 panicMode(){
        // https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 100f;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition (randomDirection, out navHit, 100f, -1);
        return navHit.position;
    }
}
