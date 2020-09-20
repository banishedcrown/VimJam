using UnityEngine;
using System.Collections;
public class MoveTest : MonoBehaviour
{
    public GameObject pointer;
    private bool spawnedPointer = false;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            if (!spawnedPointer)
            {
                w.z = 0;
                GameObject.Instantiate(pointer, w, Quaternion.identity);
                spawnedPointer = true;
            }
            GetComponent<NavMeshAgent2D>().destination = w;
        }
        else
        {
            spawnedPointer = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("trigger entered!");
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(gameObject);
        }
    }
}