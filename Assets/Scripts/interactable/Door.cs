using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractive
{
    [SerializeField] private float turnTime = 1;
    private Transform[] parts;
    public void Start()
    {
        parts = new Transform[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
            parts[i++] = child;
    }

    public void Interact(GameObject initiator)
    {
        Vector3 angles = new Vector3(0, -90, 0) * Mathf.RoundToInt(Vector3.Dot(transform.forward, initiator.transform.forward));
        for (int i = 0; i < parts.Length; i++)
        {
            int m = (i + 1) % 2 == 0 ? -1 : 1;
            StartCoroutine(Turner(parts[i], (parts[i].transform.localEulerAngles + angles) * m));
        }
    }

    private IEnumerator Turner(Transform transform, Vector3 angles)
    {
        float t = 0f;
        var a = transform.localRotation;
        var b = Quaternion.Euler(angles);
        while (t <= turnTime)
        {
            transform.localRotation = Quaternion.Slerp(a, b, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
