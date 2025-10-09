Shader "Unlit/DepthMaskShader"
{
    SubShader
    {
        Tags {"Queue" = "Transparent-1" }
        Pass
        {
            ZWrite On
            ColorMask 0
        }
    }
}