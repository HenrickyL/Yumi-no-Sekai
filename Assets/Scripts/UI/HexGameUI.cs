using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell, hoverCell, previosCurrentCell;
	private Color colorHover = new Color(0,0,0,0.15f),
		 colorSelectedUnity = new Color(0,0,1,0.3f),
		 colorMove = new  Color(0.98f,0.83f,0.29f,0.5f),
		 colorMoveHovered = Color.white,
		 colorSelectedUnityHevered = Color.blue;
	HexUnit selectedUnit, previosSelected;
	
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
				else if(Input.GetKey(KeyCode.LeftShift)){
					DoPathfinding();
				}else{
					grid.ClearPath();
				}
			}
		}
	}

	void SetHighlightHover(HexCell prev, HexCell next, Color? prevColor, Color nextColor){
		if(prevColor== null || prevColor ==  Color.clear){
			prev.DisableHighlight();
		}else{
			prev.EnableHighlight(prevColor ?? Color.red);
		}
		hoverCell = next;
		hoverCell.EnableHighlight(nextColor);
	}
	void HoveredCell(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		HexCell previous = hoverCell;
		if(!previous){
			hoverCell = grid.GetCell(ray);
			return;
		}
		HexCell next = grid.GetCell(ray);
		if( hoverCell && previous && next && next != previous ){
			highlightSelectUnitMove();
			if(selectedUnit){
				bool prevInSelect = selectedUnit.MovePath.Contains(previous);
				bool nextInSelect = selectedUnit.MovePath.Contains(next);
				if(prevInSelect && next == selectedUnit.Location){
					SetHighlightHover(previous,next,colorMove, colorSelectedUnityHevered);
				}
				else if(prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,colorMove, colorMoveHovered);
				}
				else if(!prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,null, colorMoveHovered);
				}
				else if(prevInSelect && !nextInSelect){
					SetHighlightHover(previous,next,colorMove, colorHover);
				}
				else if(previous == selectedUnit.Location && nextInSelect){
					SetHighlightHover(previous,next,colorSelectedUnity, colorMoveHovered);
				}
				else{
					SetHighlightHover(previous,next,null, colorHover);
				}
			}
			else{
				SetHighlightHover(previous,next,null, colorHover);
			}
			
		}
	}

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if(currentCell ){
			ClearPreviosSelectUnit(selectedUnit);
			if(currentCell.Unit != selectedUnit){
				previosSelected = selectedUnit;
				selectedUnit = currentCell.Unit;
				highlightSelectUnitMove();
			}
		}
	}
	void highlightSelectUnitMove(){
		if(selectedUnit){
			selectedUnit.Location.EnableHighlight(colorSelectedUnity);
			foreach(var cell in selectedUnit.MovePath){
				cell.EnableHighlight(colorMove);
			}
		}
	}
	void ClearPreviosSelectUnit( HexUnit unit){
		if(unit){
			unit.Location.DisableHighlight();
			previosCurrentCell?.DisableHighlight();
			foreach(var c in unit.MovePath){
					c.DisableHighlight();
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
		if (cell != currentCell ) {
			previosCurrentCell = currentCell;
			currentCell = cell;
			return true;
		}
		return false;
	}
}