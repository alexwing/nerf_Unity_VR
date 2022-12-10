using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Utils
{


    private static string androidInternalFilesDir = null;
    private static float clicked = 0;
    private static float clicktime = 0;
    private static float clickdelay = 0.5f;

    // Start is called before the first frame update
    public static float StringToFloat(string Value)
    {
        //  Value = Value.Replace(".", ",");

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        float valueFloat = float.Parse(Value, NumberStyles.Any, ci);

        //   float valueFloat = (float)System.Convert.ToDouble(Value);
        // float valueFloat = float.Parse(Value);
        return valueFloat;
    }

    // Start is called before the first frame update
    public static int StringToInt(string Value)
    {
        //  Value = Value.Replace(".", ",");
        try
        {
            if (!String.IsNullOrEmpty(Value) && Value != "Unknown")
            {


                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                int valueInt = int.Parse(Value, NumberStyles.Any, ci);

                //   float valueFloat = (float)System.Convert.ToDouble(Value);
                // float valueFloat = float.Parse(Value);
                return valueInt;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Utils -> StringToInt   {exception}  {exception.StackTrace}");
            return 0;
        }

    }


    /// <summary>
    /// Convert string to array int
    /// </summary>
    /// <param name="s1">String of numbers</param>
    /// <param name="separator">Character to sprit in array</param>
    /// <returns></returns>
    public static int[] StringToIntArray(string s1, char separator = ',')
    {
        try
        {
            return s1.Split(separator).Select(n => Convert.ToInt32(n)).ToArray();
        }
        catch (Exception e)
        {
            Debug.Log("StringToIntArray invalid format: " + e.ToString());
            return new int[0];
        }

    }

    public static Vector2 StringToVector2(string aCol)
    {
        try
        {
            var strings = aCol.Split(',');
            Vector2 vector = new Vector2()
            {
                x = StringToFloat(strings[0]),
                y = StringToFloat(strings[1])
            };

            return vector;

        }
        catch (Exception e)
        {
            Debug.Log("StringToVector2 invalid format: " + e.ToString());
            return new Vector2(0, 0);
        }
    }

    public static string GetPath()
    {

        string screensPath;
#if UNITY_ANDROID && !UNITY_EDITOR
			                                            screensPath = Application.temporaryCachePath;       // Also you can create a custom folder, like: screensPath = "/sdcard/DCIM/RegionCapture";

#elif UNITY_IPHONE && !UNITY_EDITOR
                                                        screensPath = Application.temporaryCachePath;       // Also you can use persistent DataPath on IOS: screensPath = Application.persistentDataPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
                                                        screensPath = Application.temporaryCachePath;       // Also you can use persistent DataPath on IOS: screensPath = Application.persistentDataPath;
#elif UNITY_WSA && !UNITY_EDITOR
                                                        screensPath = Application.persistentDataPath;       // Also you can use persistent DataPath on IOS: screensPath = Application.persistentDataPath;
#else
        screensPath = Application.persistentDataPath;    // Editor Mode

#endif

        //  Debug.Log("screensPath: " + screensPath);
        return screensPath;
    }

    public static string GetAppPath()
    {
        string AppRoot = Path.GetDirectoryName(Application.dataPath);
        if (Application.isEditor)
        {
            AppRoot = Path.Combine(AppRoot, "Build\\");
        }
        return AppRoot;
    }


    public static string GetAndroidInternalFilesDir()
    {

        if (androidInternalFilesDir is null)
        {

            string[] potentialDirectories = new string[]
            {
        "/mnt/sdcard",
        "/sdcard",
        "/storage",
        "/storage/emulated/0",
        "/storage/sdcard0",
        "/storage/sdcard1"
            };

            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < potentialDirectories.Length; i++)
                {
                    if (Directory.Exists(potentialDirectories[i]))
                    {
                        return potentialDirectories[i];
                    }
                }
            }
            return "";
        }
        else
        {
            return androidInternalFilesDir;
        }
    }


    public static string JSONSerialize<T>(T obj)
    {
        string retVal = String.Empty;
        using (MemoryStream ms = new MemoryStream())
        {
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            serializer.WriteObject(ms, obj);
            var byteArray = ms.ToArray();
            retVal = System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }
        return retVal;
    }
    public static string ConvertColumnNumberToLetter(int colNumber)
    {
        try
        {
            int n = 0;
            int c = 0;
            string s = null;
            n = colNumber;
            do
            {
                c = ((n - 1) % 26);
                s = Convert.ToChar(c + 65) + s;
                n = (n - c) / 26;
            } while (n > 0);
            return s;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static bool[] ConvertByteArrayToBoolArray(byte[] bytes)
    {
        System.Collections.BitArray b = new System.Collections.BitArray(bytes);
        bool[] bitValues = new bool[b.Count];
        b.CopyTo(bitValues, 0);
        Array.Reverse(bitValues);
        return bitValues;
    }

    /// <summary>
    /// Packs a bit array into bytes, most significant bit first
    /// </summary>
    /// <param name="boolArr"></param>
    /// <returns></returns>
    public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
    {
        byte[] byteArr = new byte[(boolArr.Length + 7) / 8];
        for (int i = 0; i < byteArr.Length; i++)
        {
            byteArr[i] = ReadByte(boolArr, 8 * i);
        }
        return byteArr;
    }


    /// <summary>
    /// Reads a code of length 8 in an array of bits, padding with zeros
    /// </summary>
    /// <param name="rawbits"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    private static byte ReadByte(bool[] rawbits, int startIndex)
    {
        int n = rawbits.Length - startIndex;
        if (n >= 8)
        {
            return (byte)ReadCode(rawbits, startIndex, 8);
        }
        return (byte)(ReadCode(rawbits, startIndex, n) << (8 - n));
    }

    /// <summary>
    /// Reads a code of given length and at given index in an array of bits
    /// </summary>
    /// <param name="rawbits">The rawbits.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    private static int ReadCode(bool[] rawbits, int startIndex, int length)
    {
        int res = 0;
        for (int i = startIndex; i < startIndex + length; i++)
        {
            res <<= 1;
            if (rawbits[i])
            {
                res++;
            }
        }
        return res;
    }

    public static Color ParseColor(string aCol)
    {
        var strings = aCol.Split(',');
        Color output = Color.black;
        for (var i = 0; i < strings.Length; i++)
        {
            output[i] = StringToFloat(strings[i]) / 256;
        }
        return output;
    }


    public static Material[] MergeMaterialsArray(Material[] input1, Material[] input2)
    {
        Material[] output = new Material[input1.Length + input2.Length];
        for (int i = 0; i < output.Length; i++)
        {
            if (i >= input1.Length)
                output[i] = input2[i - input1.Length];
            else
                output[i] = input1[i];
        }
        return output;
    }

    /// <summary>
    /// returns if the class exists in the current context
    /// </summary>
    /// <param name="className">Class Name</param>
    /// <returns>class status</returns>
    public static bool ClassExist(string className)
    {
        Type type = Type.GetType(className);
        if (type != null)
        {
            return true;
        }
        return false;
    }


    private string MD5Checksum(string path)
    {

        if (!File.Exists(path))
        {
            return "";
        }


        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty);
            }
        }
    }

    public static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    /// <summary>
    /// returns if the gameobject is visible in camera
    /// </summary>
    /// <param name="c">Camera</param>
    /// <param name="go">GameObject</param>
    /// <returns>state of visibility</returns>
    public static bool IsTargetVisible(Camera c, GameObject go)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        var point = go.transform.position;
        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(point) < 0)
                return false;
        }
        return true;
    }


    /// <summary>
    /// returns if the gameobject is looking to other
    /// </summary>
    /// <param name="c">looker</param>
    /// <param name="go">targetPos</param>
    /// <returns>state of visibility</returns>
    public static bool IsLookingAtObject(Transform looker, Transform targetPos)
    {
        Vector3 dirFromAtoB = (looker.transform.position - targetPos.transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, targetPos.transform.forward);

        if (dotProd > 0.9)
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// returns if the gameobject is front to other
    /// </summary>
    /// <param name="obj1">obj1</param>
    /// <param name="obj2">obj2</param>
    /// <returns>is object is from as her pivot</returns>
    public static bool IsFrontAtObject(Transform obj1, Transform obj2)
    {
        if (Vector3.Dot(Vector3.forward, obj1.transform.InverseTransformPoint(obj2.transform.position)) < 0)
            return true;
        else
            return false;

    }
    public static bool IsFrontAtObject(Transform obj1, Vector3 obj2)
    {
        if (Vector3.Dot(Vector3.forward, obj1.transform.InverseTransformPoint(obj2)) < 0)
            return true;
        else
            return false;

    }
    public static bool IsFrontAtObject(Vector3 obj1, Vector3 obj2)
    {
        Transform obj1T = new GameObject().transform;
        obj1T.position = obj1;
        if (Vector3.Dot(Vector3.forward, obj1T.transform.InverseTransformPoint(obj2)) < 0)
            return true;
        else
            return false;

    }


    public static String GetWifiMAC()
    {

        string mac = "";
        var card = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
        if (card == null)
        {
            return "";
        }
        else
        {
            byte[] bytes = card.GetPhysicalAddress().GetAddressBytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    //This is just for formating the final result
                    mac = string.Concat(mac + ":");
                }
            }
            mac = card.GetPhysicalAddress().ToString();
            //  Debug.Log("WIFI OK, MAC: " + mac);
            return mac;
        }

    }

    public static bool IsWifiConnect()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void ShowToastMessage(string message)
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
#else
        Debug.Log($"TOAST: {message}");
#endif
    }


    /// <summary>
    /// Ramdon from a initial position
    /// </summary>
    /// <param name="obj">obj</param>
    /// <param name="XRange">XRange</param>
    /// <param name="YRange">YRange</param>
    /// <param name="ZRange">ZRange</param>
    /// <returns>Return the randomized position</returns>
    public static Transform RandomNearPosition(Transform obj, float XRange, float YRange, float ZRange)
    {

        obj.position = new Vector3(
            Random.Range(obj.position.x - XRange, obj.position.x + XRange),
            Random.Range(obj.position.y - YRange, obj.position.y + YRange),
            Random.Range(obj.position.z - ZRange, obj.position.z + ZRange)
            );

        return obj;
    }

    public static void PlaySound(AudioClip clip, Transform collision, Transform player, int DistanceSoundLimit)
    {
        if (clip == null)
        {
            return;
        }
        float cameraDistance = Vector3.Distance(player.position, collision.position);
        float normalizedValue = Mathf.InverseLerp(0, DistanceSoundLimit, cameraDistance);
        float explosionDistanceVolumen = Mathf.Lerp(1f, 0, normalizedValue);
        // First, calculate the direction to the spawn
        Vector3 spawnDirection = collision.position - player.position;
        // Then, normalize it into a unit vector
        Vector3 unitSpawnDirection = spawnDirection.normalized;

        //Debug.Log("Camera distance: " + cameraDistance + " explosion sound volumen : " + explosionDistanceVolumen);
        // Now, we can play the sound in the direction, but not position, of the spawn
        AudioSource.PlayClipAtPoint(clip, player.position + unitSpawnDirection, explosionDistanceVolumen);
    }

    public static Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }



    /// <summary>
    /// Create ramdon postion from quad limits and list of others positions
    /// </summary>
    /// <param name="quad">quad</param>
    /// <param name="otherPositions">otherPositions</param>
    /// <param name="minimalDistanceBetween">minimalDistanceBetween</param>
    /// <param name="retries">retries</param>
    /// <returns>Return the randomized position</returns>
    public static Vector3 CreateRamdomPosition(Vector4 quad, ref List<Vector3> otherPositions, float minimalDistanceBetween, int retries = 2000)
    {
        Vector3 position = new Vector3(0, 0, 0);
        bool isInRange = false;
        int retryCount = 0;
        while (!isInRange)
        {
            isInRange = true;

            //verify not in the same position with other hills with the hillSizeLimit
            Vector3 auxPosition = new Vector3(0, 0, 0);
            float auxDistance = 0;
            position = new Vector3(UnityEngine.Random.Range(quad.x, quad.y), 0, UnityEngine.Random.Range(quad.z, quad.w));
            for (int j = 0; j < otherPositions.Count; j++)
            {
                //verfiy not in the same position in hillSizeLimit range
                float distance = Vector3.Distance(otherPositions[j], position);
                if (distance < minimalDistanceBetween)
                {
                    isInRange = false;
                    //if now position has a distance greater that before save in auxDistance
                    if (distance > auxDistance)
                    {
                        auxDistance = distance;
                        auxPosition = position;
                    }
                    //limit retry count
                    if (retryCount > retries)
                    {
                        //restore element on more distance in retries
                        position = auxPosition;

                        isInRange = true;
                       // Debug.LogWarning("Can't find a position for the hill with this hills Distance between: " + auxDistance);
                    }
                }
            }
            retryCount++;
        }


        otherPositions.Add(position);
        return position;

    }

    public static Color Darken(Color color, float darkenAmount)
    {
        return Color.Lerp(color, Color.black, darkenAmount);

    }

    /// <summary>
    ///  change the color of the object renderer copy instance
    /// </summary>
    /// <param name="painterObject">Renderer to paint</param>
    /// <param name="color">Color to paint</param>
    /// <param name="name">Shader variable</param>
    /// <returns>none</returns>    
    public static void ChangeColor(Renderer painterObject, Color color, string name = "_Color")
    {
        var propBlock = new MaterialPropertyBlock();
        painterObject.GetPropertyBlock(propBlock);
        propBlock.SetColor(name, color);
        painterObject.SetPropertyBlock(propBlock);

    }

    public static bool DoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        return false;
    }

    public static Texture2D FillColorAlpha(Texture2D tex2D, Color32? fillColor = null)
    {   
        if (fillColor ==null)
        {
            fillColor = Color.clear;
        }
        Color32[] fillPixels = new Color32[tex2D.width * tex2D.height];
        for (int i = 0; i < fillPixels.Length; i++)
        {
            fillPixels[i] = (Color32) fillColor;
        }
        tex2D.SetPixels32(fillPixels);
        return tex2D;
    }

}
