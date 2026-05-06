using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRManager : MonoBehaviour
{
    public Material liquidMaterial;
    public MixObjectColor mixedObj;
    public GameObject prefabColorCapsule;
    public List<GameObject> colorCapsules;
    public GameObject parentCapsules, initPosColorCapsule;


    public void SpawnColorizedCapsule()
    {
        GameObject obj = Instantiate(prefabColorCapsule, parentCapsules.transform);
        obj.transform.localPosition = initPosColorCapsule.transform.localPosition;
        
        GameObject foundObject = obj.GetComponentInChildren<Liquid>().gameObject;

        Material instanceMaterial = new Material(liquidMaterial);
        foundObject.GetComponent<Renderer>().material = instanceMaterial;
        Color c = mixedObj.GetPlayerMixedColor();
        foundObject.GetComponent<Renderer>().material.SetColor("_TopColor",c*0.5f);
        foundObject.GetComponent<Renderer>().material.SetColor("_FoamColor", c * 0.7f); 
        colorCapsules.Add(obj);
    }
}
