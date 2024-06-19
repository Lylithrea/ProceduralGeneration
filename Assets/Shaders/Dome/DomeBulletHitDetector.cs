using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DomeBulletHitDetector : MonoBehaviour
{

    public Material domeShader;
    public float RippleSpeed = 1f;

    public List<Vector3> hitPosition = new List<Vector3>();
    public float rippleTimer = 0;
    public float maxRippleTimer = 10;
    public float rippleThickness = 2;

    public float maxRippleDistance = 250;

    public Texture2D texture;
    public RawImage image;

    public int textureQualityWidth = 500, textureQualityHeight = 500;
    Color[] colors;

    public ComputeShader ripppler;
    private ComputeBuffer vectorBuffer;
    public RenderTexture resultTexture;
    int kernelHandle = 0;

    public void Start()
    {
        texture = new Texture2D(textureQualityWidth, textureQualityHeight);
        kernelHandle = ripppler.FindKernel("CSMain");

        resultTexture = new RenderTexture(textureQualityWidth, textureQualityHeight, 0, RenderTextureFormat.ARGB32);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();
        ripppler.SetTexture(kernelHandle, "Result", resultTexture);
        if(image != null) image.texture = texture;

        //domeShader.SetTexture("_BulletTexture", texture);
        colors = new Color[textureQualityWidth * textureQualityHeight];
        for(int i =0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }

  

        //ripppler.SetTexture(kernelHandle, "Result", texture);
    }


    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter!");
        Ray ray = new Ray(collision.transform.position, collision.transform.forward);
        Debug.Log("pos: " + collision.transform.position + " direction: " + collision.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit.textureCoord);
            hitPosition.Add(toTextureSize(hit.textureCoord));
            SetupShader();
        }
    }

    public Vector3 toTextureSize(Vector2 texCoord )
    {
        Vector3 newTexCoord = new Vector3(texCoord.x, texCoord.y, 0);
        newTexCoord.x *= textureQualityWidth;
        newTexCoord.y *= textureQualityHeight;
        newTexCoord.z = Time.time + maxRippleTimer;
        return newTexCoord;
    }

    public void SetupShader()
    {
        if (hitPosition.Count != 0)
        {
            vectorBuffer = new ComputeBuffer(hitPosition.Count, sizeof(float) * 3);
            vectorBuffer.SetData(hitPosition);
        }
        ripppler.SetBuffer(kernelHandle, "vectors", vectorBuffer);
        ripppler.SetInt("totalPoints", hitPosition.Count);
        ripppler.SetFloat("maxRippleDistance", maxRippleDistance);
        ripppler.SetFloat("maxTime", maxRippleTimer);
        ripppler.SetFloat("rippleThickness", rippleThickness);
        rippleTimer = 0;
    }

    public void Update()
    {
        if (rippleTimer <= maxRippleTimer)
        {
            domeShader.SetFloat("_RippleTimer", rippleTimer);
            rippleTimer += RippleSpeed * Time.deltaTime;
        }
        UpdateTexture();
        if (hitPosition.Count == 0) return;
        for(int i =0; i < hitPosition.Count; i++)
        {
            if (hitPosition[i].z + maxRippleTimer + 1f < Time.time)
            {
                Debug.Log("hitpos: " + hitPosition[i].z + "  current time: " + Time.time);
                hitPosition.RemoveAt(i);
                i--;
            }
        }
        SetupShader();
    }


    public void UpdateTexture()
    {
        //List<Color> newPixels = new List<Color>(co);

        //texture.SetPixels(colors);

        /*        foreach (Vector2 point in hitPosition)
                {
                    for(int x = 0; x < textureQualityWidth; x++)
                    {
                        for(int  y = 0; y < textureQualityHeight; y++)
                        {
                            Vector2 pointPos = new Vector2(point.x * textureQualityWidth, point.y * textureQualityHeight);
                            Vector2 texturePos = new Vector2(x, y);

                            float distance = Vector2.Distance(pointPos, texturePos);

                            //Debug.Log(distance / maxRippleDistance);
                            if (distance < maxRippleDistance)
                            {
                                texture.SetPixel(x, y, new Color(1 - distance / maxRippleDistance, 1 - distance / maxRippleDistance, 1 - distance / maxRippleDistance, 1));
                            }


                        }
                    }
                }*/

        if (hitPosition.Count == 0) return;

        ripppler.SetFloat("currentTime", Time.time);

        // Dispatch the compute shader
        ripppler.Dispatch(kernelHandle, textureQualityWidth / 8, textureQualityHeight/8, 1);
        //texture = resultTexture;
        domeShader.SetTexture("_BulletTexture", resultTexture);
        //domeShader.SetTexture("_BulletTexture", texture);
        Debug.Log("Finished dispatching...");
    }


}
