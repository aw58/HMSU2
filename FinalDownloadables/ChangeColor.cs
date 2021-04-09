using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{

    public AudioClip collisionSound;
    public AudioSource soundSource;

    // Start is called before the first frame update
    void Start()
    {
        soundSource.clip = collisionSound;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider otherAsset)
    {
        this.gameObject.GetComponent<Renderer>().material.color = Color.green;
        soundSource.Play();
        Debug.Log("Played");
    }

    void OnTriggerExit(Collider otherAsset)
    {
        //this.gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
}