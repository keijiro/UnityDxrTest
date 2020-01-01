using UnityEngine;

sealed public class SphereArray : MonoBehaviour
{
    void Start()
    {
        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                var x = (i - 2) * 0.44f;
                var z = (j - 2) * 0.44f;
                var hue = (i + j * 5) / 25.0f;

                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.parent = transform;
                go.transform.localPosition = new Vector3(x, 0.2f, z);
                go.transform.localScale = Vector3.one * 0.4f;

                var mat = go.GetComponent<Renderer>().material;
                mat.SetColor("_BaseColor", Color.HSVToRGB(hue, 1, 1));
                mat.SetFloat("_Smoothness", 1 - i / 4.0f);
                mat.SetFloat("_Metallic", 1 - j / 4.0f);
            }
        }
    }

}
