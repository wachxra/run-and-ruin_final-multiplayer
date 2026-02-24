using UnityEngine;
using Unity.Netcode;
public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputRender;
    [SerializeField] private Transform turrentTransform;

    private void LateUpdate()
    {
        if (!IsOwner) { return; }

        Vector2 aimScreenPosition = inputRender.AimPosition;
        Vector2 aimWorldPostion = Camera.main.ScreenToWorldPoint(aimScreenPosition);

        turrentTransform.up = new Vector2(
            aimWorldPostion.x - turrentTransform.position.x,
            aimWorldPostion.y - turrentTransform.position.y);

    }
}