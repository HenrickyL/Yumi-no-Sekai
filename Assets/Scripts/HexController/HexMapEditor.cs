using UnityEngine;

public class HexMapEditor : MonoBehaviour
{
    public Material[] colors;
    private Material activeColor;
	int activeElevation;

    private void Awake() {
        SelectColor(0);
        SetElevation(0);
    }
   
    public void SelectColor(int index)
    {
        activeColor = colors[index];
    }

    public void EditCell(HexCell cell, HexGrid hexGrid)
    {
        cell.material = activeColor;
        cell.Elevation = activeElevation;
        hexGrid.Refresh();
    }
    public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}
}
