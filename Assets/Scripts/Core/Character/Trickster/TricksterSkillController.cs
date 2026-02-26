using UnityEngine;
using Unity.Netcode;

public class TricksterSkillController : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            UseSkillServerRpc(1);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            UseSkillServerRpc(2);

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            UseSkillServerRpc(3);
    }

    [ServerRpc]
    void UseSkillServerRpc(int skillIndex)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value == CharacterRole.Runner)
            {
                var runner = client.PlayerObject
                    .GetComponentInChildren<RunnerController>();

                if (runner == null) return;

                if (skillIndex == 1)
                {
                    Debug.Log("Trickster used Slow");
                }
                else if (skillIndex == 2)
                {
                    runner.ForceJump();
                    Debug.Log("Trickster forced Jump");
                }
                else if (skillIndex == 3)
                {
                    runner.ForceSlide();
                    Debug.Log("Trickster forced Slide");
                }
            }
        }
    }
}