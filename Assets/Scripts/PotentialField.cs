using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotentialField : MonoBehaviour
{

    [SerializeField] private Ship owner = null;
    [SerializeField] private float constant;
    [SerializeField] private float exponent;


    private void Start()
    {
        AIMgr.instance.potentialFields.Add(this);
    }

    public Vector3 CalcForce(Ship s)
    {
        float d = Vector3.Magnitude(s.transform.position - gameObject.transform.position);
        if (d < 0.00001f) return Vector3.zero;
        Vector3 f = Vector3.Normalize(gameObject.transform.position - s.transform.position);
        return constant * Mathf.Pow(d, exponent) * f;
    }
}
