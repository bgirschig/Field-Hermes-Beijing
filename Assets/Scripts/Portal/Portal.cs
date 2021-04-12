// A portal object
// This only keeps a reference to the portal target and triggers teleport on collider events.
// Teleport and portal transform logic happen in PortalItem

using UnityEngine;

[ExecuteInEditMode()]
public class Portal : MonoBehaviour
{
    public Transform target;
    public Portal nextPortal;

    public static Portal current;

    void OnTriggerEnter (Collider other) {
        PortalItem otherItem = other.GetComponent<PortalItem>();
        if (otherItem == null) return;
        if (otherItem.isTrigger && nextPortal != null) nextPortal.activate();
        if (otherItem.teleport) otherItem.teleportToTwin();
	}

    void OnDrawGizmos() {
        if (target != null) {
            Gizmos.color = current == this ? Color.magenta : new Color(0.4f, 0, 0.4f);
            Vector3 offsetPoint = transform.TransformPoint(new Vector3(0,0,1));
            Vector3 targetOffsetPoint = target.TransformPoint(new Vector3(0,0,-1));
            Gizmos.DrawLine(transform.position, offsetPoint);
            Gizmos.DrawLine(offsetPoint, targetOffsetPoint);
            Gizmos.DrawLine(targetOffsetPoint, target.transform.position);
        }

        if (nextPortal != null && target != null) {
            Gizmos.color = new Color(0.5f,0.5f,1f);
            Gizmos.DrawLine(target.transform.position, nextPortal.transform.position);
        }
    }

    /// <summary>Make this portal the current active one.
    /// PortalItems will be positionned relative to this portal</summary>
    public void activate() {
        if (current != null) current.deactivate();
        current = this;
    }

    public void deactivate() {
        if (current == this) current = null;
    }
}
