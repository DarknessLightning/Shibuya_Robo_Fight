using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public Animator animator;
    public AnimationClip attack;
    public AnimationClip heal;
    public AnimationClip charge;
    public AnimationClip signal;

    public float timingForAttack = 0f;
    public float timingForHit = 0f;

    public void PlaySpecialSkill()
    {
        animator.SetTrigger("Charge");
        animator.SetBool("Signal", true);
    }

    public void EndSpecialSkill()
    {
        animator.SetBool("Signal", false);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayHit()
    {
        animator.SetTrigger("Hit");
    }

    public void PlayHeal()
    {
        animator.SetTrigger("Heal");
    }

    public void PlayCharge()
    {
        animator.SetTrigger("Charge");
    }


    public void finishAttack()
    {
        if (FightManager.instance != null)
        {
            FightManager.instance.NextResolve(3);
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
