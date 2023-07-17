using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static bool useRandomSeed;
    public static int seed;
    public static int renderDist = 2;

    public void onRandomSeedToggleChange()
    {
        GameObject randomSeed = GameObject.Find("Random Seed Toggle");
        if (randomSeed.GetComponent<Toggle>().isOn)
        {
            GameObject.Find("Seed").SetActive(false);
            useRandomSeed = true;
        }
        else
        {
            Find("Seed").SetActive(true);
            useRandomSeed = false;
        }
    }

    public void onSeedChange()
    {
        try
        {
            seed = int.Parse(GameObject.Find("Seed").GetComponent<TMP_InputField>().text);
        }
        catch(System.FormatException)
        {
            seed = 0;
        }

        if (seed == 0) useRandomSeed = true;
    }


    public void onRenderDistChange()
    {
        try
        {
            renderDist = int.Parse(GameObject.Find("Render Distance").GetComponent<TMP_InputField>().text);
            print(renderDist);
        }
        catch (System.FormatException)
        {
            renderDist = 4;
            print(renderDist);
        }
    }

    static GameObject Find(string search)
    {
        var scene = SceneManager.GetActiveScene();
        var sceneRoots = scene.GetRootGameObjects();

        GameObject result = null;
        foreach (var root in sceneRoots)
        {
            if (root.name.Equals(search)) return root;

            result = FindRecursive(root, search);

            if (result) break;
        }

        return result;
    }

    static GameObject FindRecursive(GameObject obj, string search)
    {
        GameObject result = null;
        foreach (Transform child in obj.transform)
        {
            if (child.name.Equals(search)) return child.gameObject;

            result = FindRecursive(child.gameObject, search);

            if (result) break;
        }

        return result;
    }

}
