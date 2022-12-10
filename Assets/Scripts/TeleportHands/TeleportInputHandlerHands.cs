/************************************************************************************

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using UnityEngine;


/// <summary>
/// When this component is enabled, the player will be able to aim and trigger teleport behavior using Oculus Touch controllers.
/// </summary>
public class TeleportInputHandlerHands : TeleportInputHandler
{
    public Transform LeftHand;
    public Transform RightHand;

    public static bool aim = false;
    public static bool teleport = false;
    public static bool teleportLaunched = false;


    public void aimClosed()
    {
        aim = false;

    }
    public void aimOpened()
    {
        aim = true;
    }
    public void teleportClosed()
    {
        teleport = false;
        teleportLaunched = false;

    }
    public void teleportOpened()
    {
        teleport = true;
        teleportLaunched = false;
    }

    public void Update()
    {

    }

    /// <summary>
    /// Based on the input mode, controller state, and current intention of the teleport controller, return the apparent intention of the user.
    /// </summary>
    /// <returns></returns>
    public override LocomotionTeleport.TeleportIntentions GetIntention()
    {
        if (!isActiveAndEnabled)
        {
            return global::LocomotionTeleport.TeleportIntentions.None;
        }
        if (aim && !teleportLaunched && teleport)
        {

            Debug.Log("TELEPORT ");
            aim = false;
            teleport = false;
            teleportLaunched = true;

            /*LocomotionTeleport locomotionTeleport = GetComponent<LocomotionTeleport>();
            if (locomotionTeleport != null)
            {
                    StartCoroutine(locomotionTeleport.AimStateCoroutine());
            }*/
            //	LocomotionTeleport.DoTeleport();

            return LocomotionTeleport.TeleportIntentions.Teleport;
        }
        else
        {
            if (aim)
            {
                Debug.Log("AIM teleport");
                return LocomotionTeleport.TeleportIntentions.Aim;
            }
            else
            {
                return LocomotionTeleport.TeleportIntentions.None;
            }


        }
    }


    public override void GetAimData(out Ray aimRay)
    {
        Transform t = RightHand;
        aimRay = new Ray(t.position, -t.right);
    }
}

