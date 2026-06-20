using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public Animator animator;
    public AnimationClip attack;
    public AnimationClip heal;
    public AnimationClip charge;
    public AnimationClip signal;
    public AnimationClip destruction;
    public AnimationClip brag;
    public AnimationClip lose;

    public float timingForAttack = 0f;
    public float timingForHit = 0f;
    public float timingForDestruction = 0f;
    public float timingForLaugh = 0f;

    [Header("Sound and Visual Effects")]
    public CharacterSoundEffect SFX;
    public CharacterVisualEffect VFX;
    public Transform ground;
    public Transform hand;

    public IEnumerator PlaySoundEffect(AudioClip clip, float wait)
    {
        yield return new WaitForSeconds(wait);
        AudioManager.instance.PlaySfx(clip);
    }

    public IEnumerator PlayVisualEffect(GameObject FX, Transform titik, float waitAmount)
    {
        yield return new WaitForSeconds(waitAmount);
        GameObject partikel = Instantiate(FX, titik.position + FX.transform.position, Quaternion.Euler(Vector3.zero));
        partikel.transform.rotation = transform.rotation * FX.transform.localRotation;
        partikel.transform.localScale = Vector3.Scale(transform.lossyScale, partikel.transform.localScale);
        yield return new WaitForSeconds(3f - waitAmount);
        Destroy(partikel);
    }

    public void PlaySpecialSkill(bool activate)
    {
        animator.SetTrigger("Charge");
        animator.SetBool("Signal", activate);
        StartCoroutine(SpecialSkillSfx());
        StartCoroutine(PlayVisualEffect(
            VFX.energizeFx,
            ground,
            VFX.energizeDelay));
    }

    public IEnumerator SpecialSkillSfx()
    {
        yield return PlaySoundEffect(SFX.Energize, SFX.timingForCharge);
        yield return new WaitForSeconds(charge.length);
        yield return PlaySoundEffect(SFX.Signal, SFX.timingForSignal);
    }

    public void EndSpecialSkill()
    {
        animator.SetBool("Signal", false);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
        StartCoroutine(PlaySoundEffect(
            SFX.Attack,
            SFX.timingForAttack));
        StartCoroutine(PlayVisualEffect(
            VFX.attackFx, 
            hand, 
            VFX.attackDelay));
    }

    public void PlayHit()
    {
        animator.SetTrigger("Hit");
        StartCoroutine(PlaySoundEffect(
            SFX.Hurt, 0));
    }

    public void PlayHeal()
    {
        animator.SetTrigger("Heal");
        StartCoroutine(PlaySoundEffect(
            SFX.Heal, 
            SFX.timingForHeal));
        StartCoroutine(PlayVisualEffect(
            VFX.healFx,
            ground,
            VFX.healDelay));
    }

    public void PlayCharge()
    {
        animator.SetTrigger("Charge");
        StartCoroutine(PlaySoundEffect(
            SFX.Energize, 
            SFX.timingForCharge));
        StartCoroutine(PlayVisualEffect(
            VFX.energizeFx,
            ground,
            VFX.energizeDelay));
    }

    public void PlayFame()
    {
        animator.SetTrigger("Fame");
        StartCoroutine(PlaySoundEffect(
            SFX.Fame,
            SFX.timingForFame));
    }

    public void PlayDestruction()
    {
        animator.SetTrigger("Destruction");
        StartCoroutine(PlaySoundEffect(
            SFX.Destruction,
            SFX.timingForDestruction));
    }

    public void PlayBuyCard()
    {
        animator.SetTrigger("Card");
    }

    public void PlayLose()
    {
        animator.SetTrigger("Lose");
    }

    public void finishAttack()
    {
        if (FightManager.instance != null)
        {
            //FightManager.instance.NextResolve(3);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
