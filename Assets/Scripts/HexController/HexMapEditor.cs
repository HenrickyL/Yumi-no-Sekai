using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

enum OptionalToggle {
		Ignore, Yes, No
}

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;
	private int activeElevation = 0;
    private bool applyElevation = true;
    int brushSize;
    public bool showUI = false;
    OptionalToggle riverMode;
    bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;
	int activeWaterLevel;
	bool applyWaterLevel = true;
	int activeTerrainTypeIndex;
	private int IOHeader = 1;


    void Update () {
		if (
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		) {
			HandleInput();
		}
		else {
			previousCell = null;
		}
	}
    void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if(previousCell && previousCell != currentCell){
                ValidateDrag(currentCell);
            }else{
                isDrag = false;
            }
			EditCells(currentCell);
            previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}
	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}
    private void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

    
    public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}
    public void EditCell(HexCell cell)
    {
        if(cell){
           if (activeTerrainTypeIndex >= 0) 
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
            if (applyElevation) 
                cell.Elevation = activeElevation;
			if (applyWaterLevel) 
				cell.WaterLevel = activeWaterLevel;
            if(riverMode == OptionalToggle.No)
                cell.RemoveRiver();
            else if(isDrag && riverMode == OptionalToggle.Yes){
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					otherCell.SetOutgoingRiver(dragDirection);
				}
            }
        }
    }
    public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

    public void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
    }
    public void ShowUI (bool visible) {
		showUI = visible;
	}
    public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}
	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}
	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}
	public void Save () {
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (
			BinaryWriter writer =
				new BinaryWriter(File.Open(path, FileMode.Create))
		) {
			//future editions
			writer.Write(IOHeader);
			//
			hexGrid.Save(writer);
		}
	}

	public void Load () {
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (
			
			BinaryReader reader =
				new BinaryReader(File.OpenRead(path))
		) {
			//
			int header = reader.ReadInt32();
			//
			if (header <= IOHeader) {
				hexGrid.Load(reader,header);
				HexMapCamera.ValidatePosition();
			}else{
				Debug.LogWarning("Unknown map format " + header);
			}
		}
	}
    
}
