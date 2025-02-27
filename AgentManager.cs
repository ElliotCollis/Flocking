using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentManager : MonoBehaviour
{
    public static AgentManager instance = null;

    public delegate void AgentUpdate();
    public static event AgentUpdate UpdateAgents;
    public EnemyPooler enemyPooler;

    public Transform AgentObjectsFolder;
    public GameObject PlayerPrefab;
    public PlayerAgent player = null;
    public LayerMask LinecastMask;

    public List<Agent> allAgents = new List<Agent>();
    public List<Collider2D> allColliders = new List<Collider2D>();

    public Hashtable hashtable;

    // Data for hash table calculations; hide in inspector later :)
    public int widthOfMap = 100; // update off level gen later
    public int heightOfMap = 100; // update of level gen later.
    public int cellSize = 25;
    public int cellWidthCount;
    public int numberOfBuckets;

    public Grid grid;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);

        enemyPooler = GetComponent<EnemyPooler>();
        // just for prototype?
        StartHashTable(widthOfMap, heightOfMap);
        //grid.StartAStar();

        //player = FindObjectOfType<PlayerAgent>();
    }

    public void StartHashTable (int width, int height)
    {
        widthOfMap = width;
        heightOfMap = height;

        //set up hashtable.
        cellWidthCount = Mathf.FloorToInt(widthOfMap > heightOfMap ? widthOfMap / cellSize : heightOfMap / cellSize) + 1; // I'm adding one just for those edge cases. Better too many than too few.
        numberOfBuckets = cellWidthCount * cellWidthCount;
        hashtable = new Hashtable(numberOfBuckets);
    }

    public void SpawnPlayer (Vector2 position)
    {
        player = Instantiate(PlayerPrefab, position, Quaternion.identity, AgentObjectsFolder).GetComponent<PlayerAgent>();
        // anything else I need to set for the player?
    }

    void FixedUpdate()
    {
        // get world data
        if (UpdateAgents != null)
        {
            //allColliders = GetColliders();
            UpdateAgents(); // Can pass data here or replace with a foreach loop.

        }
    }

    public void addAgentToHash (int hashID, Collider2D collider)
    {
        if(hashtable.ContainsKey(hashID))
        {
            List<Collider2D> currentBucket = (List<Collider2D>)hashtable[hashID];
            currentBucket.Add(collider);
            hashtable[hashID] = currentBucket; // is this ok for do I need to remove and add it again. is there poossible memory leaks here.....
            //hashtable.Remove(hashID);
            //hashtable.Add(hashID, currentBucket);
        }else
        {
            List<Collider2D> newBucket = new List<Collider2D>();
            newBucket.Add(collider);
            hashtable.Add(hashID, newBucket);
        }
    }

    public void removeAgentFromHash(int hashID, Collider2D collider)
    {
        if (hashtable.ContainsKey(hashID))
        {
            List<Collider2D> currentBucket = (List<Collider2D>)hashtable[hashID];
            if(currentBucket.Count <= 1)
            {
                hashtable.Remove(hashID);
            }else
            {
                currentBucket.Remove(collider);
                hashtable[hashID] = currentBucket;
            }
        }
        else
        {
           // print("no bucket set for this ID"); // this got called a few times. is this a problem...?
        }
    }

    public void updateAgentsHashBucket (int previousHashID, int newHashID, Collider2D collider)
    {
        removeAgentFromHash(previousHashID, collider);
        addAgentToHash(newHashID, collider);
    }

    public List<Collider2D> GetNeighbours (int hashID, Collider2D collider) // get neighbouring colliders in the hash table. rename it not get neighbours. that's already something.
    {
        // not brute force solution
        List<Collider2D> bucketColliders = new List<Collider2D>();

        // doing this quickly for the for loops, names are bad, but basically is determining the range of the for loop
        int x1 = -1;
        int x2 = 2;
        int y1 = -1;
        int y2 = 2;

        if (hashID % cellWidthCount == 0) // means our middle is on the left side?
        {
            x1 = 0; // pull our range over.
        }

        if (hashID+1 % cellWidthCount == 0) // means our middle is on the right side? basically just add one and check if we're on the left again XD
        {
            x2 = 1; 
        }

        if(hashID >= 0 && hashID < cellWidthCount) // means our middle is on the bottom
        {
            y1 = 0;
        }

        if(hashID >= (numberOfBuckets - cellWidthCount) && hashID < numberOfBuckets) // means our middle is on the top
        {
            y2 = 1;
        }

        // because the hashtable is a single list instead of a 2D array, we need to translate between the two.
        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
            {
                // why is this working when it's looking at local space and not global space haha. 
                // currently looks at (0,0), so why does this work.

                int index = (cellWidthCount * y) + x; 

                if (index >= 0 && index < numberOfBuckets) //check we're not under or over. Should this by <= numBuckets?
                {
                    // now we can get the bucket if it exsits and add everything to our list. 
                    List<Collider2D> bucket = (List<Collider2D>)hashtable[index];
                    if (bucket != null)
                    {
                        foreach (Collider2D neighbouringCollider in bucket)
                        {
                            bucketColliders.Add(neighbouringCollider);
                        }
                    }
                }
            }
        }

        return bucketColliders;

        // order of operations.
        // -1, -1
        // -1, 0
        // -1, 1
        // 0, -1
        // 0, 0
        // 0, 1
        // 1, -1
        // 1, 0
        // 1, 1
    }

    //this is now called once a frame instead of once a frame by every boid, better but can be improved with a hashset.
    List<Collider2D> GetColliders() //turn all colliders into a hashset
    {
        List<Collider2D> collider2Ds = new List<Collider2D>();

        foreach (Agent agent in allAgents)
        {
            collider2Ds.Add(agent.agentCollider);
        }

        return collider2Ds;
    }

    public bool CanSeePlayer(Agent agent)
    {
        RaycastHit2D hit = Physics2D.Linecast(agent.transform.position, player.transform.position, LinecastMask);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    
    public bool CanSeePlayerInRange (Agent agent)
    {
        if ((player.transform.position - agent.transform.position).sqrMagnitude < agent.playerDistanceSQR)
        {
            RaycastHit2D hit = Physics2D.Linecast(agent.transform.position, player.transform.position, LinecastMask);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool PlayerInRange (Agent agent)
    {
        if ((player.transform.position - agent.transform.position).sqrMagnitude < agent.playerDistanceSQR)
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        //draw the hash table
        cellWidthCount = Mathf.FloorToInt(widthOfMap > heightOfMap ? widthOfMap / cellSize : heightOfMap / cellSize) + 1; // I'm adding one just for those edge cases. Better too many than too few.
        numberOfBuckets = cellWidthCount * cellWidthCount;

        Gizmos.color = Color.blue;
        Vector3 zerozero = Vector3.zero;
        Vector3 cubeEdge = new Vector3(widthOfMap, heightOfMap, 0);

        zerozero.x = widthOfMap;
        Gizmos.DrawLine(Vector3.zero, zerozero);
        Gizmos.DrawLine(cubeEdge, zerozero);

        zerozero = Vector3.zero;
        zerozero.y = heightOfMap;
        Gizmos.DrawLine(Vector3.zero, zerozero);
        Gizmos.DrawLine(cubeEdge, zerozero);

        zerozero = Vector3.zero;

        for (int i = 0; i < cellWidthCount; i++)
        {
            zerozero.x += cellSize;
            cubeEdge = zerozero;
            cubeEdge.y = heightOfMap;
            Gizmos.DrawLine(zerozero, cubeEdge);
        }

        zerozero = Vector3.zero;
        for (int i = 0; i < cellWidthCount; i++)
        {
            zerozero.y += cellSize;
            cubeEdge = zerozero;
            cubeEdge.x = widthOfMap;
            Gizmos.DrawLine(zerozero, cubeEdge);
        }
    }
}

// this is goin to be more useful for agent pooling in the future. 