using System.Collections;
using UnityEngine;

public class TesAnimatorToVFX : MonoBehaviour
{
    public Animator animator;
    public TesTembakan vfx;

    public Transform shootPoint;
    public Transform healPoint;

    public void PlayShoot()
    {
        animator.SetTrigger("Attack");
        //StartCoroutine(vfx.playVFX(shootPoint, Partikel.shoot));
        StartCoroutine(playShoot());
    }
    public IEnumerator playShoot()
    {
        yield return new WaitForSeconds(1f);
        yield return vfx.playVFX(shootPoint.position, Partikel.shoot);
    }

    public void PlayHeal()
    {
        animator.SetTrigger("Heal");
        StartCoroutine(vfx.playVFX(healPoint.position, Partikel.heal));
    }
    public void PlayCharge()
    {
        animator.SetTrigger("Charge");
        StartCoroutine(vfx.playVFX(healPoint.position, Partikel.charge));
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Time.timeScale = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            PlayShoot();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlayHeal();
        }
        else if(Input.GetKeyDown(KeyCode.RightShift))
        {
            PlayCharge();
        }
    }
}
