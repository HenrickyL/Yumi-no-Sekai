using UnityEngine;


public class HexCell : MonoBehaviour {
    [SerializeField] public HexCoordinates coordinates;
    [SerializeField] public bool selected = false;
    [SerializeField]HexCell[] neighbors;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;
    public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}
    public bool HasIncomingRiver {
		get {
			return hasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver {
		get {
			return hasOutgoingRiver;
		}
	}

	public HexDirection IncomingRiver {
		get {
			return incomingRiver;
		}
	}

	public HexDirection OutgoingRiver {
		get {
			return outgoingRiver;
		}
	}
    public bool HasRiver {
		get {
			return hasIncomingRiver || hasOutgoingRiver;
		}
	}
    public bool HasRiverBeginOrEnd {
		get {
			return hasIncomingRiver != hasOutgoingRiver;
		}
	}

    Color color;
    public Color Color { 
        get{
            return color;
        }
        set{
            if(color == value){
                return;
            }
            color = value;
            Refresh();
        } 
    }
    public int elevation = int.MinValue;
    public int Elevation {
        get{
            return elevation;
        }
        set{
            if (elevation == value) {
				return;
			}
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            if(HexMesh.GetWithIrregularity()){
                position.y += (HexMetrics.SampleNoise(position).y*2f -1f)*
                    HexMetrics.elevationPerturbStrength;
            }
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
            Refresh();
        }
    }
    [SerializeField]public const float elevationStep = 5f;
    
    public HexCell GetNeighbor(HexDirection direction){
        return neighbors[(int) direction];
    }
    public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
	}
    public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}
    public HexEdgeType GetEdgeType(HexCell otherCell){
        return HexMetrics.GetEdgeType(elevation,otherCell.elevation);
    }
    private void Refresh () {
        if(chunk){
		    chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++) {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk) {
                    neighbor.chunk.Refresh();
                }
            }
        }
	}
    void RefreshSelfOnly () {
		chunk.Refresh();
	}

    public bool HasRiverThroughEdge (HexDirection direction) {
		return
			hasIncomingRiver && incomingRiver == direction ||
			hasOutgoingRiver && outgoingRiver == direction;
	}
    public void RemoveOutgoingRiver () {
		if (!hasOutgoingRiver) {
			return;
		}
		hasOutgoingRiver = false;
		RefreshSelfOnly();
        HexCell neighbor = GetNeighbor(outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}
    public void RemoveIncomingRiver () {
		if (!hasIncomingRiver) {
			return;
		}
		hasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}
    public void RemoveRiver () {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}
    

}