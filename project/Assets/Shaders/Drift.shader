Shader "Drift"
{
    Properties
    {
        DriftColour("Main Color", Color) = (1,1,1,1)
        DriftTexture("Base (RGB) Trans (A)", 2D) = "white" {}
    }
    Category
    {
        Offset -4, -4
        ZWrite Off
        Alphatest Greater 0
        Tags{ "Queue" = "Transparent"  "RenderType" = "Transparent" }
        SubShader
        {
            ColorMaterial AmbientAndDiffuse
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                ColorMask RGBA
                SetTexture[DriftTexture]
                {
                    Combine texture, texture * primary
                }
                SetTexture[DriftTexture]
                {
                    Combine primary * previous
                }
            }
        }
    }
    Fallback "Transparent/VertexLit", 2
}