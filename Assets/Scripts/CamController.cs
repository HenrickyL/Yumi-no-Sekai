using UnityEngine;
public class CamController : MonoBehaviour
{
    [SerializeField] HexGrid HexGrid;
    

    // Update is called once per frame
    void Update()
    {
        if(HexGrid  && HexGrid.hexMesh)
            transform.LookAt(HexGrid.hexMesh.transform);
    }
}
