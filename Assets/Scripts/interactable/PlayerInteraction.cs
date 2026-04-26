using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float range = 3;
    [SerializeField] private Transform mainCamera;

    private void Update()
    {
        Ray ray = new Ray(mainCamera.position, mainCamera.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green);
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (Physics.Raycast(ray, out var info, range, LayerMask.GetMask("Interactive")))
            {

                if (info.collider.TryGetComponent<IInteractive>(out var interactive))
                {
                    interactive.Interact(gameObject);
                }
            }
        }
    }
}
