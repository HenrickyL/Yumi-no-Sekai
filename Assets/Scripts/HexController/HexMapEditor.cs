using UnityEngine;
using UnityEngine.EventSystems;


enum OptionalToggle {
		Ignore, Yes, No
}

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;
    public Color[] colors;
    private Color activeColor;
	private int activeElevation = 0;
    private bool applyColor;
    private bool applyElevation = true;
    int brushSize;
    public bool showUI = false;
    OptionalToggle riverMode;
    bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;
	int activeWaterLevel;
	bool applyWaterLevel = true;



    private void Awake() {
        SetElevation(0);
        SelectColor(0);
    }
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

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
		if (applyColor) {
			activeColor = colors[index];
		}
    }
    public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}
    public void EditCell(HexCell cell)
    {
        if(cell){
            if (applyColor) 
                cell.Color = activeColor;
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
    
}
