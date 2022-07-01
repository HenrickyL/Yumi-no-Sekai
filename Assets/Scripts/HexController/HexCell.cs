using UnityEngine;
using System.IO;

public class HexCell : MonoBehaviour {
    [SerializeField] public HexCoordinates coordinates;
    [SerializeField]HexCell[] neighbors;
    private bool hasIncomingRiver, hasOutgoingRiver;
    private bool selected = false;
    public RectTransform uiRect;
    public HexGridChunk chunk;
	int waterLevel;
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
    public float StreamBedY {
		get {
			return
				(elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep;
		}
	}
	public int WaterLevel {
		get {
			return waterLevel;
		}
		set {
			if (waterLevel == value) {
				return;
			}
			waterLevel = value;
			ValidateRivers();
			Refresh();
		}
	}
	public bool IsUnderwater {
		get {
			return waterLevel > elevation;
		}
	}
	int terrainTypeIndex;
	public int TerrainTypeIndex {
		get {
			return terrainTypeIndex;
		}
		set {
			if (terrainTypeIndex != value) {
				terrainTypeIndex = value;
				Refresh();
			}
		}
	}
    public Color Color { 
        get{
            return HexMetrics.colors[terrainTypeIndex];
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
            RefreshPosition();
            ValidateRivers();
            Refresh();
        }
    }
	public float RiverSurfaceY {
		get {
			return
				(elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}
	public float WaterSurfaceY {
		get {
			return
				(waterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
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
    public void SetOutgoingRiver (HexDirection direction) {
		if (hasOutgoingRiver && outgoingRiver == direction) {
			return;
		}
		HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) {
			return;
		}
		RemoveOutgoingRiver();
		if (hasIncomingRiver && incomingRiver == direction) {
			RemoveIncomingRiver();
		}
		hasOutgoingRiver = true;
		outgoingRiver = direction;

		neighbor.RemoveIncomingRiver();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite();
	}

	bool IsValidRiverDestination (HexCell neighbor) {
		return neighbor && (
			elevation >= neighbor.elevation || waterLevel == neighbor.elevation
		);
	}

	void ValidateRivers () {
		if (
			hasOutgoingRiver &&
			!IsValidRiverDestination(GetNeighbor(outgoingRiver))
		) {
			RemoveOutgoingRiver();
		}
		if (
			hasIncomingRiver &&
			!GetNeighbor(incomingRiver).IsValidRiverDestination(this)
		) {
			RemoveIncomingRiver();
		}
	}

	public void Save (BinaryWriter writer) {
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
		writer.Write(hasIncomingRiver);
		writer.Write((byte)incomingRiver);
		writer.Write(hasOutgoingRiver);
		writer.Write((byte)outgoingRiver);
		
	}

	public void Load (BinaryReader reader) {
		terrainTypeIndex = reader.ReadByte();
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();
		hasIncomingRiver = reader.ReadBoolean();
		incomingRiver = (HexDirection)reader.ReadByte();
		hasOutgoingRiver = reader.ReadBoolean();
		outgoingRiver = (HexDirection)reader.ReadByte();
	}
    
	void RefreshPosition(){
		Vector3 position = transform.localPosition;
            position.y = Elevation * HexMetrics.elevationStep;
            if(HexGridChunk.GetWithIrregularity()){
                position.y += (HexMetrics.SampleNoise(position).y*2f -1f)*
                    HexMetrics.elevationPerturbStrength;
            }
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
	}

}