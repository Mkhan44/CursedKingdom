using UnityEngine;

public class SpaceTextureOverride : MonoBehaviour
{
    public Texture newTexture;   // Assign in Inspector
    public Texture newNormalMap; // Assign in Inspector

    void Start()
    {
        // Get the MeshRenderer on this GameObject
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        // Get the array of materials (these are **instances**)
        Material[] mats = renderer.materials;

        // Example: Change all 3 materials
        for (int i = 0; i < mats.Length; i++)
        {
            if (newTexture != null)
                mats[i].SetTexture("_MainTex", newTexture);

            if (newNormalMap != null)
                mats[i].SetTexture("_NormalMap", newNormalMap);
        }

        // Assign the modified array back (optional, but good practice)
        renderer.materials = mats;
    }
}