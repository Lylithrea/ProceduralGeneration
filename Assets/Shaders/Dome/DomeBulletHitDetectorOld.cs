using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DomeBulletHitDetectorOld : MonoBehaviour
{

    public Material domeShader;
    public float RippleSpeed = 1f;

    public List<Vector2> hitPosition = new List<Vector2>();
    public float rippleTimer = 0;
    public float maxRippleTimer = 10;

    public float maxRippleDistance = 250;

    public Texture2D texture;
    public RawImage image;

    public int textureQualityWidth = 500, textureQualityHeight = 500;
    Color[] colors;

    public ComputeShader compute;
    private RenderTexture renderTexture;
    int kernelHandle = 0;

    public void Start()
    {
        texture = new Texture2D(textureQualityWidth, textureQualityHeight);
        image.texture = texture;
        domeShader.SetTexture("_BulletTexture", texture);
        colors = new Color[textureQualityWidth * textureQualityHeight];
        for(int i =0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }

        kernelHandle = compute.FindKernel("CSMain");

        renderTexture = new RenderTexture(textureQualityWidth, textureQualityHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        compute.SetTexture(kernelHandle, "Result", renderTexture);
    }


    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter!");
        Ray ray = new Ray(collision.transform.position, collision.transform.forward);
        Debug.Log("pos: " + collision.transform.position + " direction: " + collision.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit.textureCoord);
            Vector2 pointPos = new Vector2(hit.textureCoord.x * textureQualityWidth, hit.textureCoord.y * textureQualityHeight);
            hitPosition.Add(pointPos);
            SetupShader();
        }
    }

    public void SetupShader()
    {
        //RWTexture2D<float4> Result;

        //RWStructuredBuffer<float2> points;
        //int amountOfPoints;
        //float maxRippleDistance;


        compute.SetFloat("maxRippleDistance", maxRippleTimer);
        compute.SetFloat("amountOfPoints", hitPosition.Count);
        ComputeBuffer points = new ComputeBuffer(hitPosition.Count, sizeof(float) * 2);
        points.SetData(hitPosition);

        compute.SetBuffer(kernelHandle, "points", points);


        rippleTimer = 0;
    }


    public void Update()
    {
        if (rippleTimer <= maxRippleTimer)
        {
            domeShader.SetFloat("_RippleTimer", rippleTimer);
            rippleTimer += RippleSpeed * Time.deltaTime;
        }

        if (hitPosition.Count == 0) return;

        compute.Dispatch(kernelHandle, textureQualityWidth / 8, textureQualityHeight / 8, 1);
        RetrieveTextureData();
        //UpdateTexture();
    }

    private void RetrieveTextureData()
    {
        // Create a temporary Texture2D to read the RenderTexture data
        Texture2D tempTexture = new Texture2D(textureQualityWidth, textureQualityHeight, TextureFormat.RGBAFloat, false);

        // Set the active RenderTexture to our renderTexture
        RenderTexture.active = renderTexture;

        // Read the render texture into the tempTexture
        texture.ReadPixels(new Rect(0, 0, textureQualityWidth, textureQualityHeight), 0, 0);
        texture.Apply();

        // Access the pixel data
        //Color[] pixels = tempTexture.GetPixels();

        //texture.SetPixels(tempTexture.GetPixels());
        //texture.Apply();

        // Clean up
        RenderTexture.active = null;
        
    }

    public void UpdateTexture()
    {
        //List<Color> newPixels = new List<Color>(co);

        texture.SetPixels(colors);

        foreach (Vector2 point in hitPosition)
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
        }
        texture.Apply();

    }


}
