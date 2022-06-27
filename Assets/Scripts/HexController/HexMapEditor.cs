using UnityEngine;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    private Color activeColor;
	private int activeElevation = 0;
    private bool applyColor;
    private bool applyElevation = true;
    int brushSize;
    public bool showUI = false;


    private void Awake() {
        SetElevation(0);
        SelectColor(0);
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
        }
    }
    public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

    public void EditCells(HexCell center, HexGrid hexGrid)
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
    
}
