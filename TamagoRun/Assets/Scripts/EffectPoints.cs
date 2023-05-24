using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPoints : MonoBehaviour
{

    public void OnEnable()
    {
        transform.GetComponentInChildren<ParticleSystem>().Play();
        //transform.GetComponentInChildren<Animator>().Play("Start");

        StartCoroutine(Disable(1.0f));
    }

    public IEnumerator Disable (float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.gameObject.SetActive(false);
    }
}
