using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell, hoverCell, previosCurrentCell, destinationCell;
	private Color colorHover = new Color(0,0,0,0.15f),
		 colorSelectedUnity = new Color(0,0,1,0.3f),
		 colorMove = new  Color(0.98f,0.83f,0.29f,0.5f),
		 colorMoveHovered = Color.white,
		 colorAttack = new Color(1,0,0,0.5f),
		 colorAttackHovered = Color.red,
		 colorSelectedUnityHevered = Color.blue;
	Color colorActiveAction, colorActiveActionHovered;
	private List<Color> actionsColors, actionColorHovered;
	
	HexUnit selectedUnit, previosSelected;
	bool isTravler = false;
	UnitActionsEnum unitAction= UnitActionsEnum.Move;
	
	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void SetActionMode(int action){
		unitAction = (UnitActionsEnum)action;
	}

	private void OnEnable() {
		actionsColors = new List<Color>(){
			colorMove,colorAttack
		};
		actionColorHovered = new List<Color>(){
			colorMoveHovered, colorAttackHovered
		};
		SetMoveMode();
	}
	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			HoveredCell();
			// CheckTravelEnd();
			HexCell currentCell = grid.GetCell(Input.mousePosition);
			if (Input.GetMouseButtonDown(0)) {
				DoSelection();
			}
			else if (selectedUnit) {
				if (Input.GetMouseButtonDown(1)) {
					DoMove(currentCell);
				}
				else if(Input.GetKey(KeyCode.LeftShift)){
					DoPathfinding();
				}else if(isTravler){
					grid.ClearPath();
					isTravler = false;
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
			HighlightSelectUnitAction();
			if(selectedUnit){
				bool prevInSelect = selectedUnit.MovePath.Contains(previous);
				bool nextInSelect = selectedUnit.MovePath.Contains(next);
				if(prevInSelect && next == selectedUnit.Location){
					SetHighlightHover(previous,next,colorActiveAction, colorSelectedUnityHevered);
				}
				else if(prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorActiveActionHovered);
				}
				else if(!prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,null, colorActiveActionHovered);
				}
				else if(prevInSelect && !nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorHover);
				}
				else if(previous == selectedUnit.Location && nextInSelect){
					SetHighlightHover(previous,next,colorSelectedUnity, colorActiveActionHovered);
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
				SwapSelectedUnit(currentCell.Unit);
				HighlightSelectUnitAction();
			}
		}
	}
	void HighlightSelectUnitAction(){
		if(selectedUnit){
			selectedUnit.Location.EnableHighlight(colorSelectedUnity);
			foreach(var cell in selectedUnit.MovePath){
				cell.EnableHighlight(colorActiveAction);
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
			isTravler = true;
			if (currentCell && selectedUnit.IsValidFullDestination(currentCell)) {
				grid.FindPath(selectedUnit.Location, currentCell, 5*selectedUnit.Speed);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoMove (HexCell cell) {
		if (grid.HasPath && cell) {
			ClearPreviosSelectUnit(selectedUnit);
			destinationCell = cell;
			// selectedUnit.Location = currentCell;
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
			SwapSelectedUnit(null);
		}else{
			destinationCell = null;
		}
	}

	void SwapSelectedUnit(HexUnit unit){
		previosSelected = selectedUnit;
		selectedUnit = unit;
	}
	void CheckTravelEnd(){
		if(selectedUnit && destinationCell){
			if(selectedUnit.Location == destinationCell){
				HighlightSelectUnitAction();
				destinationCell = null;
			}
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

	public void SetAttackMode(){
		colorActiveAction = actionsColors[1];
		colorActiveActionHovered = actionColorHovered[1];
	}
	public void SetMoveMode(){
		colorActiveAction = actionsColors[0];
		colorActiveActionHovered = actionColorHovered[0];
	}
}