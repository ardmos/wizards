using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingImageController : MonoBehaviour
{
    public Image image;

    public Sprite[] imageAssets; 

    // Start is called before the first frame update
    void Start()
    {
        image.sprite = imageAssets[Random.Range(0, 3)];
    }


}
