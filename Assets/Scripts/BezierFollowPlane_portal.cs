/// A portal-compatible version of the generic BezierFollowPlane
// Implementation is basically the same, except:
// - We watch for teleportationEvent and update the followed bezier path accordingly
// - When there is no instersecion with the current bezier, we try to find an
//   intersection with the "next bezier", "through" the portal

using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class BezierFollowPlane_portal : MonoBehaviour
{
    [Tooltip("The portal item whose teleportation will trigger the switch to the next curve")]
    public PortalItem portalItem;
    [Tooltip("The bezier path to follow")]
    public BezierPath bezierPath;
    [Tooltip("The plane to intersect with")]
    public PlaneComponent targetPlane;
    
    private BezierPortalMatcher portalMatcher;
    private BezierPath nextBezierPath;
    private PlaneComponent twinTargetPlane;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        portalItem.onTeleport.AddListener(OnTeleport);
        portalMatcher = bezierPath.GetComponent<BezierPortalMatcher>();

        // find the target's twin
        yield return 0; // wait for protal item twins to be created
        twinTargetPlane = PortalItem.findTwin<PlaneComponent>(targetPlane);
    }

    void OnTeleport(Vector3 offset) {
        bezierPath = portalMatcher.nextBezier;
        portalMatcher = bezierPath.GetComponent<BezierPortalMatcher>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bezierPath == null) return;
        if (targetPlane == null) return;
        Vector3? position;

        Plane plane = targetPlane.GetPlane();
        position = bezierPath.GetIntersection(plane);
        if (position != null) {
            // found an intersection on first try. Great!
            transform.position = (Vector3)position;
        } else {
            // No intersection found here. try with the next bezier (placed "virtually" at the end of this one)
            
            if (twinTargetPlane == null) return;
            // World position in the portal target's world
            position = portalMatcher.nextBezier.GetIntersection(twinTargetPlane.GetPlane());

            if (position != null) transform.position = Portal.current.InverseTeleportPoint((Vector3)position);
        }

    }
}
