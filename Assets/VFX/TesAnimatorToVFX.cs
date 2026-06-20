using System.Collections;
using UnityEngine;

public class TesAnimatorToVFX : MonoBehaviour
{
    public Animator animator;
    public TesTembakan vfx;

    public Transform shootPoint;
    public Transform healPoint;

    public string trigger;
    public GameObject fx;
    public bool justOneFx;
    public Transform titik;
    public float waitAmount;
    public bool isPlayingAnimation;

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
    public void PlayOneAnimation()
    {
        animator.SetTrigger(trigger);
        StartCoroutine(playVfx(fx, titik));
    }

    public IEnumerator playVfx(GameObject FX, Transform titik)
    {
        isPlayingAnimation = true;
        yield return new WaitForSeconds(waitAmount);
        GameObject partikel = Instantiate(FX, titik.position, Quaternion.Euler(Vector3.zero));
        partikel.transform.rotation = transform.rotation * FX.transform.localRotation;
        partikel.transform.localScale = Vector3.Scale(transform.lossyScale, partikel.transform.localScale);
        yield return new WaitForSeconds(3f - waitAmount);
        Destroy(partikel);
        isPlayingAnimation = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Time.timeScale = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (justOneFx)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isPlayingAnimation)
            {
                PlayOneAnimation();
            }
            return;
        }
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
