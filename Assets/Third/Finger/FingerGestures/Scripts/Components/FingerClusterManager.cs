using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu( "FingerGestures/Components/Finger Cluster Manager" )]
public class FingerClusterManager : MonoBehaviour 
{
    [System.Serializable]
    public class Cluster
    {
        public int Id = 0;
        public float StartTime = 0;
        public FingerGestures.FingerList Fingers = new FingerGestures.FingerList();

        public void Reset()
        {
            Fingers.Clear();
        }
    }

    public float ClusterRadius = 250.0f; // spatial grouping
    public float TimeTolerance = 0.5f;  // temporal grouping

    int lastUpdateFrame = -1;
    int nextClusterId = 1;
    List<Cluster> clusters; // active clusters
    List<Cluster> clusterPool;
    FingerGestures.FingerList fingersAdded;
    FingerGestures.FingerList fingersRemoved;

    public FingerGestures.IFingerList FingersAdded
    {
        get { return fingersAdded; }
    }
    
    public FingerGestures.IFingerList FingersRemoved
    {
        get { return fingersRemoved; }
    }

    public List<Cluster> Clusters
    {
        get { return clusters; }
    }

    public List<Cluster> GetClustersPool() { return clusterPool; }
    
    void Awake()
    {
        clusters = new List<Cluster>();
        clusterPool = new List<Cluster>();
        fingersAdded = new FingerGestures.FingerList();
        fingersRemoved = new FingerGestures.FingerList();
    }

	public void Update()
    {
        // already updated this frame, skip
        if( lastUpdateFrame == Time.frameCount )
            return;

        lastUpdateFrame = Time.frameCount;

        fingersAdded.Clear();
        fingersRemoved.Clear();

        for( int i = 0; i < FingerGestures.Instance.MaxFingers; ++i )
        {
            FingerGestures.Finger finger = FingerGestures.GetFinger( i );

            if( finger.IsDown )
            {
                // new touch?
                if( !finger.WasDown )
                {
                    //Debug.Log( "ADDED " + finger );
                    fingersAdded.Add( finger );
                }
            }
            else
            {
                // lifted off finger
                if( finger.WasDown )
                {
                    //Debug.Log( "REMOVED " + finger );
                    fingersRemoved.Add( finger );
                }
            }
        }

        for (int k = 0; k < fingersRemoved.Count; k++)
        {
            FingerGestures.Finger finger = fingersRemoved[k];
            for (int i = clusters.Count - 1; i >= 0; --i)
            {
                Cluster cluster = clusters[i];

                if (cluster.Fingers.Remove(finger))
                {
                    // retire clusters that no longer have any fingers left
                    if (cluster.Fingers.Count == 0)
                    {
                        //Debug.Log( "Recycling cluster " + cluster.Id );

                        // remove from active clusters list
                        clusters.RemoveAt(i);

                        // move back to pool
                        clusterPool.Add(cluster);
                    }
                }
            }
        }


        for (int k = 0; k < fingersAdded.Count; k++)
        {
            FingerGestures.Finger finger = fingersAdded[k];
            // try to add finger to existing cluster
            Cluster cluster = FindExistingCluster(finger);

            // no valid active cluster found for that finger, create a new cluster
            if (cluster == null)
            {
                cluster = NewCluster();
                cluster.StartTime = finger.StarTime;
                //Debug.Log( "Created NEW cluster " + cluster.Id + " for " + finger );                
            }
            else
            {
                //Debug.Log( "Found existing cluster " + cluster.Id + " for " + finger );
            }

            // add finger to selected cluster
            cluster.Fingers.Add(finger);
        }
        // add new fingers
    }

    public Cluster FindClusterById( int clusterId )
    {
        for(int i=0;i< clusters.Count;i++)
        {
            Cluster c = clusters[i];
            if (c.Id == clusterId)
            {
                return c;
            }
        }
        return null;
    }

    Cluster NewCluster()
    {
        Cluster cluster = null;

        if( clusterPool.Count == 0 )
        {
            cluster = new Cluster();
        }
        else
        {
            int lastIdx = clusterPool.Count - 1;
            cluster = clusterPool[lastIdx];
            cluster.Reset();
            clusterPool.RemoveAt( lastIdx );
        }

        // assign a new ID
        cluster.Id = nextClusterId++;

        // add to active clusters
        clusters.Add( cluster );    // add cluster to active clusters list

        //Debug.Log( "Created new finger cluster #" + cluster.Id );
        return cluster;
    }

    // Find closest cluster within radius
    Cluster FindExistingCluster( FingerGestures.Finger finger )
    {
        Cluster best = null;
        float bestSqrDist = float.MaxValue;

        // account for higher pixel density touch screens
        float adjustedClusterRadius = FingerGestures.GetAdjustedPixelDistance( ClusterRadius );

        foreach( Cluster cluster in clusters )
        {
            float elapsedTime = finger.StarTime - cluster.StartTime;

            // temporal grouping criteria
            if( elapsedTime > TimeTolerance )
                continue;

            Vector2 centroid = cluster.Fingers.GetAveragePosition();
            float sqrDist = Vector2.SqrMagnitude( finger.Position - centroid );

            if( sqrDist < bestSqrDist && sqrDist < ( adjustedClusterRadius * adjustedClusterRadius ) )
            {
                best = cluster;
                bestSqrDist = sqrDist;
            }
        }

        return best;
    }
}
