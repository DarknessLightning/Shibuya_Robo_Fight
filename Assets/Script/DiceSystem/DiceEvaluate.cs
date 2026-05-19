using NUnit.Framework;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DiceEvaluate : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 startingPos;

    private Vector3 endPos;

    public float force = 5f;
    public float torque = 10f;

    public bool locked = false;

    public int GetTopNumber()
    {
        float maxDot = -Mathf.Infinity;
        int topNumber = -1;

        CheckSide(transform.up, 1, ref maxDot, ref topNumber);
        CheckSide(-transform.up, 6, ref maxDot, ref topNumber);

        CheckSide(transform.forward, 5, ref maxDot, ref topNumber);
        CheckSide(-transform.forward, 2, ref maxDot, ref topNumber);

        CheckSide(transform.right, 3, ref maxDot, ref topNumber);
        CheckSide(-transform.right, 4, ref maxDot, ref topNumber);

        return topNumber;
    }

    void CheckSide(Vector3 dir, int number, ref float maxDot, ref int topNumber)
    {
        float dot = Vector3.Dot(dir, Vector3.up);

        if (dot > maxDot)
        {
            maxDot = dot;
            topNumber = number;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        startingPos = transform.localPosition;
        resetEnd();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Result: " + GetTopNumber());
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
        }*/
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse is on Dice. Result: " + GetTopNumber().ToString());
    }

    public void RollDice()
    {
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 randomForce =
            Vector3.forward * force +
            Random.insideUnitSphere * 2f;

        rb.AddForce(randomForce, ForceMode.Impulse);

        Vector3 randomTorque = Random.insideUnitSphere * torque;
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    public void back2Starting()
    {
        transform.localPosition = startingPos;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void back2End()
    {
        transform.localPosition = endPos;
    }

    public void setPosAndRot(Vector3 pos, Vector3 rot)
    {
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(rot);
    }

    public bool IsMoving()
    {
        bool moving = rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f;

        return moving;
    }

    public void setEnd()
    {
        endPos = transform.localPosition;
    }

    public void resetEnd()
    {
        endPos = startingPos;
    }

}
