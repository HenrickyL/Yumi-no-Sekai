using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell, hoverCell;
	bool hoverSelected = false;

	HexUnit selectedUnit;

	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			HoveredCell();
				if (Input.GetMouseButtonDown(0)) {
					DoSelection();
				}
				else if (selectedUnit) {
					if (Input.GetMouseButtonDown(1)) {
						DoMove();
					}
					else {
						DoPathfinding();
					}
				}
		}
	}
	void HoveredCell(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		HexCell previous = hoverCell;
		if(!previous){
			hoverCell = grid.GetCell(ray);
			return;
		}
		HexCell next = grid.GetCell(ray);
		if(next != previous && previous){
			previous.DisableHighlight();
			previous=hoverCell;	
			hoverCell = next;
			hoverCell.EnableHighlight(new Color(0,0,0,0.15f));
		}
	}
	void SelectUnit(){

	}

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell) {
			selectedUnit = currentCell.Unit;
			foreach(var cell in selectedUnit.MovePath){
				
			}
		}
	}

	void DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell && selectedUnit.IsValidDestination(currentCell)) {
				grid.FindPath(selectedUnit.Location, currentCell, 24);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoMove () {
		if (grid.HasPath) {
//			selectedUnit.Location = currentCell;
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
		}
	}

	bool UpdateCurrentCell () {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell) {
			currentCell = cell;
			return true;
		}
		return false;
	}
}