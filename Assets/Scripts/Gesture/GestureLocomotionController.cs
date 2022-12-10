/************************************************************************************

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// Simply aggregates accessors.
/// </summary>
public class GestureLocomotionController : MonoBehaviour
{
    public OVRCameraRig CameraRig;


    void Start()
    {
        if(CameraRig == null)
        {
            CameraRig = FindObjectOfType<OVRCameraRig>();
        }
        Assert.IsNotNull(CameraRig);
#if UNITY_EDITOR
        OVRPlugin.SendEvent("locomotion_controller", (SceneManager.GetActiveScene().name == "Locomotion").ToString(), "sample_framework");
#endif
	}
}
