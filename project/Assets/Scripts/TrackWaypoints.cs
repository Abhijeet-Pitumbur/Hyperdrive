using System.Collections.Generic;
using UnityEngine;

public class TrackWaypoints : MonoBehaviour
{

    public List<Transform> waypoints;
    private Transform[] path;
    private Vector3 currentWaypoint, previousWaypoint;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        path = GetComponentsInChildren<Transform>();
        waypoints = new List<Transform>();
        for (int i = 1; i < path.Length; i++)
        { waypoints.Add(path[i]); }
        for (int i = 0; i < waypoints.Count; i++)
        {
            currentWaypoint = waypoints[i].position;
            previousWaypoint = Vector3.zero;
            if (i != 0)
            { previousWaypoint = waypoints[i - 1].position; }
            else if (i == 0)
            { previousWaypoint = waypoints[waypoints.Count - 1].position; }
            Gizmos.DrawLine(previousWaypoint, currentWaypoint);
            Gizmos.DrawSphere(currentWaypoint, 4f);
        }
    }

}