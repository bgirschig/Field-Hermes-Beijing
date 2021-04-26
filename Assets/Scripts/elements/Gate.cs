/// Controls the opening / closing of "gates"
// (elements that hide the next portal to avoid 'infinite mirrors' effect)

// The CloseGate can be called automatically on teleport when attached to a Portal / linked to a Portal
// The OpenGate must be called from another behaviour (eg. an ColliderEventTrigger)

using UnityEngine;

public class Gate : MonoBehaviour
{
    [Tooltip("When the player reaches this portal, the gate will be reset")]
    public Portal portal;

    Animator animator;
    private bool opened;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (portal == null) portal = GetComponent<Portal>();
        if (portal != null) {
            portal.onTeleport.AddListener((GameObject item) => CloseGate());
        }
    }

    public void CloseGate() {
        if (!opened) return;
        opened = false;
        animator.Play("gate-close", 0, 0f);
    }

    public void OpenGate() {
        if (opened) return;
        opened = true;
        animator.Play("gate-open", 0, 0f);
    }
}
