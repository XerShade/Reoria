using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reoria.Controllers.Objects.Actors.Players
{
    public class PlayerController : ActorController
    {
        [SerializeField]
        float tick;

        [SerializeField]
        float target;

        // Update is called once per frame
        void Update()
        {
            if(isLocalPlayer)
            {
                tick += Time.deltaTime;

                if (tick > 5f)
                {
                    target = Random.Range(0f, 2f);

                    if (target <= 1f)
                    {
                        GetComponent<PlayerCameraController>().CmdSetTarget(gameObject);
                    }
                    else
                    {
                        GetComponent<PlayerCameraController>().CmdSetTargetPosition(new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
                    }

                    tick = 0f;
                }
            }
        }
    }
}
