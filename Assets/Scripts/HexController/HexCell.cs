using UnityEngine;


public class HexCell : MonoBehaviour {
    [SerializeField] public HexCoordinates coordinates;
    [SerializeField] public Material material;
    [SerializeField] public bool selected = false;
    [SerializeField]HexCell[] neighbors;
    [SerializeField] public int elevation;
    public RectTransform uiRect;

    public Color Color { get{return this.material.color;} }
    public int Elevation {
        get{
            return elevation;
        }
        set{
            elevation = value;

            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = elevation * -HexMetrics.elevationStep;
			uiRect.localPosition = uiPosition;
        }
    }
    [SerializeField]public const float elevationStep = 5f;
    
    public HexCell GetNeighbor(HexDirection direction){
        return neighbors[(int) direction];
    }
    public void SetNeighbor (HexDirection direction, HexCell cell) {
        if(neighbors[(int)direction] != null){
            Debug.Log("already instantatate!");
        }
		neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
	}
    public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

}