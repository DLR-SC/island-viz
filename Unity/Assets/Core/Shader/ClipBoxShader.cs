using UnityEngine;
 
 [ExecuteAlways]
public class ClipBoxShader : MonoBehaviour
{
    void Update()
    {
        Shader.SetGlobalVector("_Sphere", new Vector4(transform.position.x, transform.position.y, transform.position.z, transform.localScale.x / 2));
    }
}