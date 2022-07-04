using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private void LateUpdate() {
        Camera main = Camera.main;
        transform.LookAt(transform.position + main.transform.forward);
    }
}
