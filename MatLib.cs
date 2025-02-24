using UnityEngine;

namespace Dinghies
{
    /// <summary>
    /// Manages a material library to easily access and assign materials that are not correctly decompiled (e.g Crest materials)
    /// </summary>
    public class MatLib
    {   
        public static Material convexHull;
        public static Material foam;
        public static Material objectInteraction;
        public static Material water4;
        public static Material mask;
        public static Material overflow;
        //public static Material flags;

        public static void RegisterMaterials()
        {   //save the materials from the cog to the variables so that I can then easily assign them to the dinghy's gameobjects
            GameObject cog = GameObject.Find("BOAT medi small (40)");
            Transform mediSmall = cog.transform.Find("medi small");
            convexHull = mediSmall.Find("mask").GetComponent<MeshRenderer>().sharedMaterial;
            foam = cog.transform.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial;
            objectInteraction = cog.transform.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().sharedMaterial;
            water4 = mediSmall.Find("damage_water").GetComponent<MeshRenderer>().sharedMaterial;
            mask = mediSmall.Find("mask_splash").GetComponent<MeshRenderer>().sharedMaterial;
            overflow = cog.transform.Find("overflow particles").GetComponent<Renderer>().sharedMaterial;
        }
    }
}
