using Reoria.Framework.Objects;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    [SerializeField]
    private float timer = 0f;
    [SerializeField]
    private float nextTimer = 0f;

    private void Start()
    {
        gameObject.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), gameObject.transform.position.z);
        nextTimer = Random.Range(3f, 7f);
        MainCamera.Instance.Target = gameObject;
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextTimer)
        {
            gameObject.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), gameObject.transform.position.z);

            nextTimer = Random.Range(3f, 7f);
            timer = 0f;
        }
    }
}
